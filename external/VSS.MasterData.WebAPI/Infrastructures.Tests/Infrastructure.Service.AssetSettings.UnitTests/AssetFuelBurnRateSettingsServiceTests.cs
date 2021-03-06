﻿using Interfaces;
using AutoMapper;
using CommonModel.AssetSettings;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Request.AssetSettings;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonModel.Exceptions;
using ClientModel.Interfaces;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using Infrastructure.Service.AssetSettings.Publisher;
using Infrastructure.Service.AssetSettings.Service;
using Infrastructure.Service.AssetSettings.Validators;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;
using CustomerRepository;
using VSS.MasterData.WebAPI.Data.Confluent;

namespace VSP.MasterData.Asset.Domain.UnitTests
{
	public class AssetFuelBurnRateSettingsServiceTests
	{
		private readonly IAssetConfigRepository _stubAssetSettingsRepository;
		private readonly IAssetSettingsListRepository _stubAssetSettingsListRepository;
		private readonly IAssetSettingsPublisher _assetSettingsPublisher;
		private readonly IPublisher _stubKafkaPublisher;
		private readonly IEnumerable<IRequestValidator<AssetFuelBurnRateSettingRequest>> _assetFuelBurnRateSettingsValidators;
		private readonly IEnumerable<IRequestValidator<AssetSettingsRequestBase>> _assetSettingsValidators;
		private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestParametersValidators;
		private readonly IAssetConfigTypeRepository _stubAssetConfigTypeRepository;
		private readonly ICustomerRepository _stubCustomerRepository;
		private readonly ILoggingService _stubLoggingService;
		private readonly IMapper _mapper;
		private readonly IInjectConfig _stubInjectConfig;
		private readonly IAssetSettingsService<AssetFuelBurnRateSettingRequest, AssetFuelBurnRateSettingsDetails> _assetSettingsService;
		private readonly Guid _customerUid;
		private readonly Guid _userUid;
		private readonly ITransactions _stubTransactions;
		private readonly List<AssetConfigTypeDto> _assetConfigTypeDtos;
		private readonly List<Guid> _assetUIDs;
		private readonly IOptions<Configurations> _stubConfigurations;
		private List<AssetSettingsDto> _assetSettingsDtos;

		public AssetFuelBurnRateSettingsServiceTests()
		{
			this._customerUid = Guid.NewGuid();
			this._userUid = Guid.NewGuid();
			this._assetUIDs = new List<Guid>();
			this._assetSettingsDtos = new List<AssetSettingsDto>();
			for (int i = 0; i < 20; i++)
			{
				this._assetUIDs.Add(Guid.NewGuid());
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					this._assetSettingsDtos.Add(new AssetSettingsDto
					{
						AssetConfigUID = Guid.NewGuid(),
						AssetUID = this._assetUIDs[i + j],
						InsertUTC = DateTime.UtcNow,
						UpdateUTC = DateTime.UtcNow,
						StartDate = DateTime.Now.Date,
						TargetType = j == 0 ? AssetTargetType.WorkingBurnRateinLiPerHour.ToString() : AssetTargetType.IdlingBurnRateinLiPerHour.ToString(),
						TargetValue = 5500.00 - j
					}); ;
				}
			}
			this._stubAssetConfigTypeRepository = Substitute.For<IAssetConfigTypeRepository>();
			this._stubAssetSettingsListRepository = Substitute.For<IAssetSettingsListRepository>();
			this._stubAssetSettingsListRepository.FetchValidAssetUIds(Arg.Any<List<string>>(), Arg.Any<AssetSettingsListRequestDto>()).Returns(
				x =>
				{
					var request = (AssetSettingsListRequestDto)x[1];
					if (request.CustomerUid == _customerUid.ToString("N") && request.UserUid == _userUid.ToString("N"))
					{
						return _assetUIDs.Where(y => ((List<string>)x[0]).Contains(y.ToString())).Select(z => z.ToString());
					}
					else
					{
						return new List<string>();
					}
				});
			this._assetConfigTypeDtos = new List<AssetConfigTypeDto>();
			this._assetConfigTypeDtos.AddRange(
				new List<AssetConfigTypeDto> {
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 0,
						ConfigTypeName = "IdletimeHours"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 1,
						ConfigTypeName = "RuntimeHours"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 2,
						ConfigTypeName = "Runtime"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 3,
						ConfigTypeName = "OdometerinKmsPerWeek"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 4,
						ConfigTypeName = "BucketVolumeinCuMeter"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 5,
						ConfigTypeName = "PayloadinTonnes"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 6,
						ConfigTypeName = "CycleCount"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 7,
						ConfigTypeName = "VolumeinCuMeter"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 8,
						ConfigTypeName = "IdlingBurnRateinLiPerHour"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 9,
						ConfigTypeName = "WorkingBurnRateinLiPerHour"
					},
					new AssetConfigTypeDto
					{
						AssetConfigTypeID = 10,
						ConfigTypeName = "PayloadPerCycleInTonnes"
					}
				});
			this._stubTransactions = Substitute.For<ITransactions>();
			this._stubAssetSettingsRepository = Substitute.For<IAssetConfigRepository>();
			this._stubKafkaPublisher = Substitute.For<IPublisher>();
			this._stubInjectConfig = Substitute.For<IInjectConfig>();
			this._stubInjectConfig.ResolveKeyed<IPublisher>(Arg.Any<string>()).Returns(this._stubKafkaPublisher);

			this._stubAssetSettingsRepository.FetchAssetConfig(Arg.Any<List<string>>(), Arg.Any<AssetSettingsDto>()).Returns(x => Task.FromResult(_assetSettingsDtos.Where(y => ((List<string>)x[0]).Contains(y.AssetUID.ToString()))));
			this._stubAssetConfigTypeRepository.FetchByConfigTypeNames(Arg.Any<AssetConfigTypeDto>()).Returns(callInfo => Task.FromResult(_assetConfigTypeDtos.Where(x => ((AssetConfigTypeDto)callInfo[0]).ConfigTypeNames.Contains(x.ConfigTypeName))));

			this._stubLoggingService = Substitute.For<ILoggingService>();
			this._stubLoggingService.CreateLogger(this.GetType());

			this._stubCustomerRepository = Substitute.For<ICustomerRepository>();

			this._serviceRequestParametersValidators = new List<IRequestValidator<IServiceRequest>>
			{
				new CustomerUidValidator(_stubCustomerRepository, this._stubLoggingService),
				new UserUidValidator(this._stubLoggingService)
			};
			this._assetSettingsValidators = new List<IRequestValidator<AssetSettingsRequestBase>>
			{
				new AssetUIDsValidator(this._stubAssetSettingsListRepository, this._stubLoggingService)
			};
			this._assetFuelBurnRateSettingsValidators = new List<IRequestValidator<AssetFuelBurnRateSettingRequest>>
			{
				new BurnRateTargetValueValidator(this._stubLoggingService)
			};

			this._stubConfigurations = Options.Create<Configurations>(new Configurations
			{
				KafkaSettings = new KafkaSettings
				{
					PublisherTopics = new PublisherTopics
					{
						AssetWeeklySettingsTopicName = "VSS.VISIONLINK.INTERFACES.ASSETWEEKLYTARGETS",
						UserAssetSettingsTopicName = "VSS.VISIONLINK.INTERFACES.USERASSETTARGETS",
						AssetSettingsTopicName = "VSS.VISIONLINK.INTERFACES.ASSETTARGETS",
						UserAssetWeeklySettingsTopicName = "VSS.VISIONLINK.INTERFACES.USERASSETWEEKLYTARGETS"
					}
				}
			});

			this._assetSettingsPublisher = new AssetSettingsPublisher(_stubConfigurations, _stubTransactions, _stubLoggingService);
			var mapperConfig = new MapperConfiguration(config =>
			{
				config.CreateMap<AssetSettingsListDto, AssetSettingsDetails>();
				config.CreateMap<AssetSettingsDto, AssetSettingsResponse>();
			});
			this._mapper = mapperConfig.CreateMapper();
			this._assetSettingsService = new AssetBurnRateSettingsService(_stubAssetSettingsRepository, _stubAssetConfigTypeRepository, _assetSettingsPublisher, _assetSettingsValidators, _assetFuelBurnRateSettingsValidators, _serviceRequestParametersValidators, _mapper, _stubTransactions, _stubLoggingService);
		}

		[Fact]
		public void Fetch_ValidRequest_ReturnValidResponse()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Take(3).Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				}
			};

			var result = this._assetSettingsService.Fetch(request).Result;
			Assert.NotNull(result);
			Assert.True(result.AssetSettingsLists.Count == 3);
		}

		[Fact]
		public void Fetch_InvalidAssetUIDInAssetUIDs_ThrowsDomainException()
		{

			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Take(2).Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				}
			};
			var invalidAssetUID = Guid.NewGuid().ToString();
			try
			{
				request.AssetUIds.Add(invalidAssetUID);
				var result = this._assetSettingsService.Fetch(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.InvalidAssetUID);
				Assert.Equal(domainEx.Errors.First().Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), invalidAssetUID));
			}
		}

		[Fact]
		public void Fetch_AssetUIDNull_ThrowsDomainException()
		{

			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = null,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				WorkTargetValue = 1000,
				IdleTargetValue = 100,
				CustomerUid = this._customerUid,
				UserUid = this._userUid
			};
			try
			{
				var result = this._assetSettingsService.Fetch(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.AssetUIDListNull);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.AssetUIDListNull));
			}
		}

		//TODO: Change the method names appropriately
		[Fact]
		public void Fetch_InvalidAllAssetUIDs_ThrowsDomainException()
		{

			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = null,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				CustomerUid = this._customerUid,
				UserUid = this._userUid
			};
			var invalidAssetUID = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
			try
			{
				request.AssetUIds = invalidAssetUID.ToList();
				var result = this._assetSettingsService.Fetch(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.InvalidAssetUID);
				Assert.Equal(domainEx.Errors.First().Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), string.Join(",", invalidAssetUID)));
			}
		}

		[Fact]
		public void Fetch_CustomerUIDNull_ThrowsDomainException()
		{

			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Select(x => x.ToString())),
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				CustomerUid = null,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				UserUid = this._userUid
			};
			try
			{
				var result = this._assetSettingsService.Fetch(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.CustomerUIDNull);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.CustomerUIDNull));
			}
		}

		[Fact]
		public void Fetch_UserUIDNull_ThrowsDomainException()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Select(x => x.ToString())),
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				CustomerUid = this._customerUid,
				UserUid = null
			};
			try
			{
				var result = this._assetSettingsService.Fetch(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.UserUIDNull);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.UserUIDNull));
			}
		}



		[Fact]
		public void Save_ValidRequest_ThreeOldRecords_ThreeNewRecords_ReturnValidResponse()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Take(3).Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				StartDate = DateTime.Now.AddDays(1)
			};

			var result = this._assetSettingsService.Save(request).Result;

			Assert.NotNull(result);
			Assert.True(result.AssetSettingsLists.Count == 3);
			this._stubTransactions.Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void Save_ValidRequest_ThreeNewRecords_ReturnValidResponse()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Take(3).Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				StartDate = DateTime.Now
			};

			var assetSettingsDtoCloned = new List<AssetSettingsDto>(this._assetSettingsDtos);
			this._assetSettingsDtos.RemoveAll(x => request.AssetUIds.Contains(x.AssetUID.ToString()));

			var result = this._assetSettingsService.Save(request).Result;

			Assert.NotNull(result);
			this._stubTransactions.Execute(Arg.Any<List<Action>>());
			this._assetSettingsDtos = assetSettingsDtoCloned;
		}

		[Fact]
		public void Save_ValidRequest_ThreeOldRecords_ReturnValidResponse()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Take(3).Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				StartDate = DateTime.Now.Date
			};

			var assetSettingsDtoCloned = new List<AssetSettingsDto>(this._assetSettingsDtos);
			var assetSettingsDtos = this._assetSettingsDtos.FindAll(y => request.AssetUIds.Contains(y.AssetUID.ToString()));

			var result = this._assetSettingsService.Save(request).Result;

			Assert.NotNull(result);
			this._stubTransactions.Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void Save_ValidRequest_OneOldRecord_TwoNewRecords_ReturnValidResponse()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Take(3).Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				StartDate = DateTime.Now.AddDays(1)
			};

			var assetSettingsDtoCloned = new List<AssetSettingsDto>(this._assetSettingsDtos);
			var assetSettingsDto = this._assetSettingsDtos.Where(x => request.AssetUIds.Contains(x.AssetUID.ToString()));
			Guid modifiedAssetUID = Guid.Empty;
			foreach (var assetSettings in assetSettingsDto.Take(1))
			{
				modifiedAssetUID = assetSettings.AssetUID;
				assetSettings.StartDate = DateTime.Now.AddDays(-1);
			}
			this._assetSettingsDtos.RemoveAll(x => assetSettingsDto.Where(y => y.AssetUID != modifiedAssetUID).Select(y => y.AssetUID).Contains(x.AssetUID));

			var result = this._assetSettingsService.Save(request).Result;

			Assert.NotNull(result);
			
			this._stubTransactions.Received(1).Execute(Arg.Any<List<Action>>());

			this._assetSettingsDtos = assetSettingsDtoCloned;

		}

		[Fact]
		public void Save_InvalidAssetUIDInAssetUIDs_ThrowsDomainException()
		{

			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Take(2).Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour, 100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				},
				IdleTargetValue = 100,
				WorkTargetValue = 1000
			};
			var invalidAssetUID = Guid.NewGuid().ToString();
			try
			{
				request.AssetUIds.Add(invalidAssetUID);
				var result = this._assetSettingsService.Save(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.InvalidAssetUID);
				Assert.Equal(domainEx.Errors.First().Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), invalidAssetUID));
			}
		}

		[Fact]
		public void Save_AssetUIDNull_ThrowsDomainException()
		{

			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = null,
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				}
			};
			try
			{
				var result = this._assetSettingsService.Save(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.AssetUIDListNull);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.AssetUIDListNull));
			}
		}

		[Fact]
		public void Save_InvalidAllAssetUIDs_ThrowsDomainException()
		{

			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = null,
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour, 100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				}
			};
			var invalidAssetUID = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
			try
			{
				request.AssetUIds = invalidAssetUID.ToList();
				var result = this._assetSettingsService.Save(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;

				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.InvalidAssetUID);
				Assert.Equal(domainEx.Errors.First().Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), string.Join(",", invalidAssetUID)));
			}
		}

		[Fact]
		public void Save_CustomerUIDNull_ThrowsDomainException()
		{

			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Select(x => x.ToString())),
				CustomerUid = null,
				UserUid = this._userUid,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour, 100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				}
			};
			try
			{
				var result = this._assetSettingsService.Save(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.CustomerUIDNull);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.CustomerUIDNull));
			}
		}

		[Fact]
		public void Save_UserUIDNull_ThrowsDomainException()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = null,
				IdleTargetValue = 100,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour, 100 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				}
			};
			try
			{
				var result = this._assetSettingsService.Save(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.UserUIDNull);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.UserUIDNull));
			}
		}

		[Fact]
		public void Save_IdleTargetValueNegative_ThrowsDomainException()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = -1,
				WorkTargetValue = 1000,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  -1 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  1000 }
				}
			};
			try
			{
				var result = this._assetSettingsService.Save(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.IdleValueShouldNotBeNegative);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.IdleValueShouldNotBeNegative));
			}
		}

		[Fact]
		public void Save_WorkTargetValueLessThanIdleTargetValue_ThrowsDomainException()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = 1000,
				WorkTargetValue = 100,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  1000 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  100 }
				}
			};
			try
			{
				var result = this._assetSettingsService.Save(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.WorkValueShouldBeLessThanIdleValue);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.WorkValueShouldBeLessThanIdleValue));
			}
		}

		[Fact]
		public void Save_WorkTargetValueAndIdleTargetValueIsZero_GivesOK()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = 0,
				WorkTargetValue = 0,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  0 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  0 }
				}
			};

			var result = this._assetSettingsService.Save(request).Result;

			Assert.NotNull(result);
			this._stubTransactions.Execute(Arg.Any<List<Action>>());
		}

		[Fact]
		public void Save_WorkTargetValueIsZero_ThrowsDomainException()
		{
			var request = new AssetFuelBurnRateSettingRequest
			{
				AssetUIds = new List<string>(this._assetUIDs.Select(x => x.ToString())),
				CustomerUid = this._customerUid,
				UserUid = this._userUid,
				IdleTargetValue = -1,
				WorkTargetValue = 0,
				TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour,  -1 },
					{ AssetTargetType.WorkingBurnRateinLiPerHour,  0 }
				}
			};
			try
			{
				var result = this._assetSettingsService.Save(request).Result;
			}
			catch (AggregateException aggregateEx)
			{
				Assert.NotNull(aggregateEx.InnerException);
				var domainEx = (DomainException)aggregateEx.InnerException;
				Assert.NotNull(domainEx);
				Assert.NotNull(domainEx.Errors);
				Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.IdleValueShouldNotBeNegative);
				Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.IdleValueShouldNotBeNegative));
			}
		}
	}
}
