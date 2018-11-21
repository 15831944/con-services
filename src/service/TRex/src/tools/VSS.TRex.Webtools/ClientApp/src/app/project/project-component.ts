import { Component } from '@angular/core';
import { ProjectExtents, DesignDescriptor, SurveyedSurface, Design, Machine, ISiteModelMetadata, MachineEventType, MachineDesign } from './project-model';
import { ProjectService } from './project-service';
import { DisplayMode } from './project-displaymode-model';
import { VolumeResult } from '../project/project-volume-model';
import { CombinedFilter, SpatialFilter, AttributeFilter, FencePoint} from '../project/project-filter-model';

@Component({
  selector: 'project',
  templateUrl: './project-component.html',
  providers: [ProjectService]
//  styleUrls: ['./project.component.less']
})
export class ProjectComponent {
  private zoomFactor: number = 0.2;

  public currentGridName: string;

  public projectUid: string;
  public mode: number = 0;
  public pixelsX: number = 850;
  public pixelsY: number = 500;

  public base64EncodedTile: string = '';

  public displayModes: DisplayMode[] = [];
  public displayMode: DisplayMode = new DisplayMode();

  public eventTypes: MachineEventType[] = [];
  public eventType: MachineEventType = new MachineEventType();

  public projectExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);
  public tileExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);

  public projectStartDate: Date = new Date();
  public projectEndDate: Date = new Date();

  public projectVolume: VolumeResult = new VolumeResult(0, 0, 0, 0, 0);

  public mousePixelLocation: string;
  public mouseWorldLocation: string;

  private mouseWorldX: number = 0;
  private mouseWorldY: number = 0;

  public timerStartTime: number = performance.now();
  public timerEndTime: number = performance.now();
  public timerTotalTime: number = 0;

  public applyToViewOnly: boolean = false;

  public surveyedSurfaceFileName: string = "";
  public surveyedSurfaceAsAtDate: Date = new Date();
  public surveyedSurfaceOffset: number = 0;

  public newSurveyedSurfaceGuid: string = "";
  public surveyedSurfaces: SurveyedSurface[] = [];

  public designFileName: string = "";
  public designOffset: number = 0.0;
  public designUID: string = "";

  public newDesignGuid: string = "";
  public designs: Design[] = [];
  public machineDesigns: MachineDesign[] = [];

  public machines: Machine[] = [];
  public machine: Machine = new Machine();

  public existenceMapSubGridCount: number = 0;

  public machineColumns: string[] = 
  ["id",
  "internalSiteModelMachineIndex",
  "name",
  "machineType",
  "deviceType",
  "machineHardwareID",
  "isJohnDoeMachine",
  "lastKnownX",
  "lastKnownY",
  "lastKnownPositionTimeStamp",
  "lastKnownDesignName",
  "lastKnownLayerId"];

  public machineColumnNames: string[] =
   ["ID",
    "Index", // "internalSiteModelMachineIndex"
    "Name",
    "Type",
    "Device",
    "Hardware ID",
    "John Doe",
    "Last Known X",
    "Last Known Y",
    "Last Known Date",
    "Last Known Design",
    "Last Known Layer"];

  public projectMetadata: ISiteModelMetadata;

  public allProjectsMetadata: ISiteModelMetadata[] = [];

  public machineEvents: string[] = [];
  public machineEventsStartDate: Date = new Date(1980, 1, 1, 0, 0, 0, 0);
  public machineEventsEndDate: Date = new Date(2100, 1, 1, 0, 0, 0, 0);
  public maxMachineEventsToReturn: number = 100;

  public profilePath: string = "M0 0 L200 500 L400 0 L600 500 L800 0 L1000 500";

  public updateFirstPointLocation: boolean = false;
  public updateSecondPointLocation: boolean = false;

  public firstPointX: number = 0.0;
  public firstPointY: number = 0.0;

  public secondPointX: number = 0.0;
  public secondPointY: number = 0.0;


constructor(
    private projectService: ProjectService
  ) { }

  ngOnInit() { 
    this.projectService.getDisplayModes().subscribe((modes) => {
      modes.forEach(mode => this.displayModes.push(mode));
      this.displayMode = this.displayModes[0];
    });

    this.projectService.getMachineEventTypes().subscribe((types) => {
      types.forEach(type => this.eventTypes.push(type));
      this.eventType = this.eventTypes[0];
    });

    this.getAllProjectMetadata();

    this.switchToMutable();
  }

  public selectProject(): void {
    this.getProjectExtents();
    this.getExistenceMapSubGridCount();
    this.getSurveyedSurfaces();
    this.getDesigns();
    this.getMachines();
    this.getMachineDesigns();

    // Sleep for half a second to allow the project extents result to come back, then zoom all
    setTimeout(() => this.zoomAll(), 250);
  }

  public setProjectToZero(): void {
    this.projectUid = "00000000-0000-0000-0000-000000000000";
  }

  public getProjectExtents(): void {
    this.projectService.getProjectExtents(this.projectUid).subscribe(extent => {
      this.projectExtents = new ProjectExtents(extent.minX, extent.minY, extent.maxX, extent.maxY);
    });

    this.projectService.getProjectDateRange(this.projectUid).subscribe(dateRange => {
      this.projectStartDate = dateRange.item1;
      this.projectEndDate = dateRange.item1;
    });
  }

  public displayModeChanged(event : any): void {
    this.mode = this.displayMode.item1;
    this.getTileXTimes(1);
  }

  public async getTileAsync(projectUid: string,
    mode: number,
    pixelsX: number,
    pixelsY: number,
    tileExtents: ProjectExtents): Promise<string> {

    var vm = this;
    return new Promise<string>((resolve) => this.projectService.getTile(projectUid, mode, pixelsX, pixelsY, tileExtents)
        .subscribe(tile => {
          vm.base64EncodedTile = 'data:image/png;base64,' + tile.tileData;
          resolve();
        }
        ));
  }

  public performNTimesSync(doSomething: (numRemaining:number) => Promise<any>, count: number): void {
      let result: Promise<any> = doSomething(count);
      result.then(() => {
      if (count > 0) {
        doSomething(count - 1);
      }
    });  
  }

  public testAsync() {
    this.tileExtents = new ProjectExtents(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
    this.performNTimesSync(() => this.getTileAsync(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.tileExtents), 10);
  }

  public getTile() : void {
    // If there is no project bail...
    if (this.projectUid == undefined)
      return;

    // Make sure the displayed tile extents is updated
    this.tileExtents = new ProjectExtents(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
    this.projectService.getTile(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.tileExtents)
      .subscribe(tile => {
        this.base64EncodedTile = 'data:image/png;base64,' + tile.tileData;
        this.updateTimerCompletionTime();      
      });
  }

  public zoomAllXTimes(xTimes: number): void {
    this.timeSomething(() => this.performNTimes(() => this.zoomAll(), xTimes));
  }

  public zoomAll(): void {
    // Ensure our project bounds are up to date
    this.getProjectExtents(); 

    // Square up the tileExtents to match the aspect ratio between the displayed image and the requested world area

    let tileExtents = new ProjectExtents(this.projectExtents.minX, this.projectExtents.minY, this.projectExtents.maxX, this.projectExtents.maxY);

    if ((this.projectExtents.sizeX() / this.projectExtents.sizeY()) > (this.pixelsX / this.pixelsY)) {
      // The project extents are 'wider' than the display, make the tile extent taller to compensate
      let ratioFraction = (this.projectExtents.sizeX() / this.projectExtents.sizeY()) / (this.pixelsX / this.pixelsY);
      tileExtents.expand(0, ratioFraction - 1);
    } else {
      // The project extents are 'shorter' than the display, , make the tile extent wider to compensate
      let ratioFraction = (this.projectExtents.sizeY() / this.projectExtents.sizeX()) / (this.pixelsY / this.pixelsX) ;
      tileExtents.expand(ratioFraction - 1, 0);
    }

    // Assign modified extents into bound model 
    this.tileExtents = tileExtents;
    this.getTile();
  }

  public zoomIn(): void {
    this.tileExtents.shrink(this.zoomFactor, this.zoomFactor);
    this.getTileXTimes(1);
  }

  public zoomOut(): void {
    this.tileExtents.expand(this.zoomFactor, this.zoomFactor);
    this.getTileXTimes(1);
  }

  public panLeft(): void {
    this.tileExtents.panByFactor(-this.zoomFactor, 0.0);
    this.getTileXTimes(1);
  }

  public panRight(): void {
    this.tileExtents.panByFactor(this.zoomFactor, 0.0);
    this.getTileXTimes(1);
  }

  public panUp(): void {
    this.tileExtents.panByFactor(0, this.zoomFactor);
    this.getTileXTimes(1);
  }

  public panDown(): void {
    this.tileExtents.panByFactor(0, -this.zoomFactor);
    this.getTileXTimes(1);
  }

  public getSimpleFullVolume(): void {
    var filter = new CombinedFilter();

    if (this.applyToViewOnly) {
      filter.spatialFilter.coordsAreGrid = true;

      filter.spatialFilter.isSpatial = true;
      filter.spatialFilter.Fence.isRectangle = true;

      filter.spatialFilter.Fence.Points = [];
      filter.spatialFilter.Fence.Points.push(new FencePoint(this.tileExtents.minX, this.tileExtents.minY));
      filter.spatialFilter.Fence.Points.push(new FencePoint(this.tileExtents.minX, this.tileExtents.maxY));
      filter.spatialFilter.Fence.Points.push(new FencePoint(this.tileExtents.maxX, this.tileExtents.maxY));
      filter.spatialFilter.Fence.Points.push(new FencePoint(this.tileExtents.maxX, this.tileExtents.minY));
    }

    this.projectService.getSimpleFullVolume(this.projectUid, filter).subscribe(volume =>
      this.projectVolume = new VolumeResult(volume.cut, volume.cutArea, volume.fillArea, volume.fillArea, volume.totalCoverageArea));
  }

  private updateMouseLocationDetails(offsetX : number, offsetY: number): void {
    let localX = offsetX;
    let localY = this.pixelsY - offsetY;

    this.mouseWorldX = this.tileExtents.minX + offsetX * (this.tileExtents.sizeX() / this.pixelsX);
    this.mouseWorldY = this.tileExtents.minY + (this.pixelsY - offsetY) * (this.tileExtents.sizeY() / this.pixelsY);

    this.mousePixelLocation = `${localX}, ${localY}`;
    this.mouseWorldLocation = `${this.mouseWorldX.toFixed(3)}, ${this.mouseWorldY.toFixed(3)}`;
  }

  public onMouseOver(event: any): void {
    this.updateMouseLocationDetails(event.offsetX, event.offsetY);
  }

  public onMouseMove(event: any): void {
    this.updateMouseLocationDetails(event.offsetX, event.offsetY);
  }

  public onMouseWheel(event: any): void {
    let panDeltaX = this.zoomFactor * (this.mouseWorldX - this.tileExtents.centerX());
    let panDeltaY = this.zoomFactor * (this.mouseWorldY - this.tileExtents.centerY());

    if (event.deltaY < 0) {
      // Zooming in 
      this.tileExtents.panByDelta(panDeltaX, panDeltaY);
      this.zoomIn();
    } else if (event.deltaY > 0) {
      // Zooming out
      this.tileExtents.panByDelta(-panDeltaX, -panDeltaY);
      this.zoomOut();
    }
  }

  public timeSomething(doSomething: () => void): void {
    this.timerStartTime = performance.now();
    doSomething();
    this.updateTimerCompletionTime();
  }

  public performNTimes(doSomething: () => void, count: number): void {
    for (var i = 0; i < count; i++) {
      doSomething();
    }
  }

  public getTileXTimes(xTimes: number): void {
    this.timeSomething(() => this.performNTimes(() => this.getTile(), xTimes));
  }

  private updateTimerCompletionTime() : void {
    this.timerEndTime = performance.now();
    this.timerTotalTime = this.timerEndTime - this.timerStartTime;
  }

  public testJSONParameter(): void {
    var filter: CombinedFilter = new CombinedFilter();
    filter.spatialFilter = new SpatialFilter();
    filter.attributeFilter = new AttributeFilter();

    this.projectService.testJSONParameter(filter).subscribe(x => x);
  }

  public addADummySurveyedSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = `C:/temp/${performance.now()}/SomeFile.ttm`;

    this.projectService.addSurveyedSurface(this.projectUid, descriptor, new Date(), this.tileExtents).subscribe(
      uid => {
        this.newSurveyedSurfaceGuid = uid.id;
        this.getSurveyedSurfaces();
      });
  }

  public addNewSurveyedSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.surveyedSurfaceFileName;
    descriptor.offset = this.surveyedSurfaceOffset;

    this.projectService.addSurveyedSurface(this.projectUid, descriptor, this.surveyedSurfaceAsAtDate, new ProjectExtents(0, 0, 0, 0)).subscribe(
      uid => {
        this.newSurveyedSurfaceGuid = uid.id;
        this.getSurveyedSurfaces();
      });

  }

  public getSurveyedSurfaces(): void {
    var result: SurveyedSurface[] = [];
    this.projectService.getSurveyedSurfaces(this.projectUid).subscribe(
      surveyedsurfaces => {
        surveyedsurfaces.forEach(ss => result.push(ss));
        this.surveyedSurfaces = result;
      });  
  }

  public deleteSurveyedSurface(surveyedSurface : SurveyedSurface): void {
    this.projectService.deleteSurveyedSurface(this.projectUid, surveyedSurface.id).subscribe(x =>
      this.surveyedSurfaces.splice(this.surveyedSurfaces.indexOf(surveyedSurface), 1));
  }

  public addADummyDesign(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = `C:/temp/${performance.now()}/SomeFile.ttm`;

    this.projectService.addDesign(this.projectUid, descriptor, this.tileExtents).subscribe(
      uid => {
        this.newDesignGuid = uid.designId;
        this.getDesigns();
      });
  }

  public addNewDesign(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.designFileName;
    descriptor.offset = this.designOffset;

    this.projectService.addDesign(this.projectUid, descriptor, new ProjectExtents(0, 0, 0, 0)).subscribe(
      uid => {
        this.newDesignGuid = uid.designId;
        this.getDesigns();
      });
  }

  public addNewDesignFromS3(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.designFileName;
    descriptor.designId = this.designUID;

    this.projectService.addDesignFromS3(this.projectUid, descriptor, new ProjectExtents(0, 0, 0, 0)).subscribe(
      uid => {
        this.newDesignGuid = uid.designId;
        this.getDesigns();
      });
  }

    public getDesigns(): void {
    var result: Design[] = [];
    this.projectService.getDesigns(this.projectUid).subscribe(
      designs => {
        designs.forEach(design => result.push(design));
        this.designs = result;
      });  
    }

  public deleteDesign(design: Design): void {
    this.projectService.deleteDesign(this.projectUid, design.id).subscribe(x =>
      this.designs.splice(this.designs.indexOf(design), 1));
  }

  public getMachineDesigns(): void {
    var result: MachineDesign[] = [];
    this.projectService.getMachineDesigns(this.projectUid).subscribe(
      machineDesigns => {
        machineDesigns.forEach(machineDesign => result.push(machineDesign));
        this.machineDesigns = result;
      });
  }

  public getMachines(): void {
    var result: Machine[] = [];
    this.projectService.getMachines(this.projectUid).subscribe(
      machines => {
        machines.forEach(machine => result.push(machine));
        this.machines = result;
      });
  }

  public getExistenceMapSubGridCount(): void {
    this.projectService.getExistenceMapSubGridCount(this.projectUid).subscribe(count =>
      this.existenceMapSubGridCount = count);
  }

  public getAllProjectMetadata(): void {
    var result: ISiteModelMetadata[] = [];
    this.projectService.getAllProjectMetadata().subscribe(
      metadata => {
        metadata.forEach(data => result.push(data));
        this.allProjectsMetadata = result;
      });
  }

  public projectMetadataChanged(event: any): void {
    this.projectUid = this.projectMetadata.id;
    this.selectProject();
  }

  public updateAllProjectsMetadata(): void {
    this.getAllProjectMetadata();
  }

  public updateDisplayedMachineEvents() {
    // Request the first 100 events of the selected event type from the selected site model and machine
    var result: string[] = [];

    var startDate: Date = this.machineEventsStartDate;
    if (startDate == undefined) {
      startDate = new Date(1980, 1, 1, 0, 0, 0, 0);
    }
      
    var endDate: Date = this.machineEventsEndDate;
    if (endDate == undefined) {
      endDate = new Date(2100, 1, 1, 0, 0, 0, 0);
    }

    this.projectService.getMachineEvents(this.projectUid, this.machine.id, this.eventType.item1,
      this.machineEventsStartDate, this.machineEventsEndDate, this.maxMachineEventsToReturn).subscribe(      
      events => {
        events.forEach(event => result.push(event));
        this.machineEvents = result;
      });
  }

  public eventTypeChanged(event: any): void {
    this.updateDisplayedMachineEvents();
  }

  public machineChanged(event: any): void {
    this.updateDisplayedMachineEvents();
  }

  public updateMachineEvents(): void {
    this.updateDisplayedMachineEvents();
  }

  public switchToMutable(): void {
    this.projectService.switchToMutable().subscribe();
    this.currentGridName = "Mutable";
  }

  public switchToImmutable(): void {
    this.projectService.switchToImmutable().subscribe();
    this.currentGridName = "Immutable";
  }

  // Requests a computed profile and then transforms the resulting XYZS points into a SVG Path string
  // with m move instruction at the first vertex, and at any vertex indicating a gap and line instructions
  // between all others
  public drawProfileLineForDesign(startX: number, startY: number, endX: number, endY: number) {
    var profileCanvasHeight:number = 500.0;
    var profileCanvasWidth: number = 1000.0;

    var result: string = "";
    var first: boolean = true;

    return this.projectService.drawProfileLineForDesign(this.projectUid, this.designUID, startX, startY, endX, endY)
      .subscribe(points =>
      {
        var stationRange:number = points[points.length - 1].station - points[0].station;
        var stationRatio:number = profileCanvasWidth / stationRange;

        var minZ:number = 100000.0;
        var maxZ:number = -100000.0;
        points.forEach(pt => {if (pt.z < minZ) minZ = pt.z});
        points.forEach(pt => {if (pt.z > maxZ) maxZ = pt.z});

        var zRange = maxZ - minZ;
        var zRatio = profileCanvasHeight / zRange;

        points.forEach(point => {
          if (point.z > 100000) {
            // It's a gap...
            first = true;
          }
          else {
            result += (first ? "M" : "L") + point.station * stationRatio + " " + (profileCanvasHeight - (point.z - minZ) * zRatio);
            first = false;
            }
        });
        this.profilePath = result;
      });
  }

  //Draw profile line from bottom left to top right of project 
  public drawProfileBLToTR(): void {
    this.drawProfileLineForDesign(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
  }

  //Draw profile line from top left to bottom right of project 
  public drawProfileTLToBR(): void {
    this.drawProfileLineForDesign(this.tileExtents.minX, this.tileExtents.maxY, this.tileExtents.maxX, this.tileExtents.minY);
  }

  public onMapMouseClick(): void {
    if (this.updateFirstPointLocation) {
      this.firstPointX = this.mouseWorldX;
      this.firstPointY = this.mouseWorldY;
    }

    if (this.updateSecondPointLocation) {
      this.secondPointX = this.mouseWorldX;
      this.secondPointY = this.mouseWorldY;
    }
  }

  public drawProfileLineFromStartToEndPointsForDesign(): void {
    this.drawProfileLineForDesign(this.firstPointX, this.firstPointY, this.secondPointX, this.secondPointY);
  }

  // Requests a computed profile and then transforms the resulting XYZS points into a SVG Path string
  // with m move instruction at the first vertex, and at any vertex indicating a gap and line instructions
  // between all others
  public drawProfileLineForProdData(startX: number, startY: number, endX: number, endY: number) {
    var profileCanvasHeight: number = 500.0;
    var profileCanvasWidth: number = 1000.0;

    var result: string = "";
    var first: boolean = true;

    return this.projectService.drawProfileLineForProdData(this.projectUid, startX, startY, endX, endY)
      .subscribe(points => {
        var stationRange: number = points[points.length - 1].station - points[0].station;
        var stationRatio: number = profileCanvasWidth / stationRange;

        var minZ: number = 100000.0;
        var maxZ: number = -100000.0;
        points.forEach(pt => { if (pt.z < minZ) minZ = pt.z });
        points.forEach(pt => { if (pt.z > maxZ) maxZ = pt.z });

        var zRange = maxZ - minZ;
        var zRatio = profileCanvasHeight / zRange;

        points.forEach(point => {
          if (point.z > 100000) {
            // It's a gap...
            first = true;
          }
          else {
            result += (first ? "M" : "L") + point.station * stationRatio + " " + (profileCanvasHeight - (point.z - minZ) * zRatio);
            first = false;
          }
        });
        this.profilePath = result;
      });
  }

  public drawProfileLineFromStartToEndPointsForProdData(): void {
    this.drawProfileLineForProdData(this.firstPointX, this.firstPointY, this.secondPointX, this.secondPointY);
  }
}

