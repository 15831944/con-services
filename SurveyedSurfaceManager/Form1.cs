﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.Velociraptor.DesignProfiling;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.ExistenceMaps;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Services.Designs;
using VSS.VisionLink.Raptor.Services.Surfaces;
using VSS.VisionLink.Raptor.Surfaces;
using VSS.VisionLink.Raptor.ExistenceMaps;

namespace SurveyedSurfaceManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private SurveyedSurfaceServiceProxy DeployedSurveyedSurfaceService = null;
        private SurveyedSurfaceService SurveyedSurfaceService = null;

        private DesignsService DesignsService = null;

        private bool CheckConnection()
        {
            if ((DeployedSurveyedSurfaceService == null && SurveyedSurfaceService == null) || (DesignsService == null))
            {
                MessageBox.Show("Not connected to service");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!long.TryParse(txtSiteModelID.Text, out long ID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the offset
            if (!double.TryParse(txtOffset.Text, out double offset))
            {
                MessageBox.Show("Invalid design offset");
                return;
            }

            // Invoke the service to add the surveyed surface
            try
            {
                // Load the file and extract its extents
                TTMDesign TTM = new TTMDesign(SubGridTree.DefaultCellSize);
                TTM.LoadFromFile(Path.Combine(new string[] { txtFilePath.Text, txtFileName.Text }));

                BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
                TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
                TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

                if (DeployedSurveyedSurfaceService != null)
                {
                    DeployedSurveyedSurfaceService.Invoke_Add(ID, 
                                                              new DesignDescriptor(Guid.NewGuid().GetHashCode(), "", "", txtFilePath.Text, txtFileName.Text, offset),                                                          
                                                              dateTimePicker.Value,
                                                              extents);

                    throw new NotImplementedException("Existence map not set via Ignite service invocation to add a surveyes surface or design");
                }
                else
                {
                    SurveyedSurfaceService.AddDirect(ID, 
                                                     new DesignDescriptor(Guid.NewGuid().GetHashCode(), "", "", txtFilePath.Text, txtFileName.Text, offset),
                                                     dateTimePicker.Value,
                                                     extents,
                                                     out long SurveyedSurfaceID);

                    // Store the existence map for the surveyd surface for later use
                    ExistenceMaps.SetExistenceMap(ID, Consts.EXISTANCE_SURVEYED_SURFACE_DESCRIPTOR, SurveyedSurfaceID, TTM.SubgridOverlayIndex());
                }
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        /// <summary>
        /// Register services...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            // Deploy the service as a cluster singleton
            DeployedSurveyedSurfaceService = new SurveyedSurfaceServiceProxy();

            try
            { 
                DeployedSurveyedSurfaceService.Deploy();
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void btnListSurveyedSurfacesClick(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            try
            {
                // Get the site model ID
                if (!long.TryParse(txtSiteModelID.Text, out long ID))
                {
                    MessageBox.Show("Invalid Site Model ID");
                    return;
                }

                SurveyedSurfaces ss = DeployedSurveyedSurfaceService != null ? DeployedSurveyedSurfaceService.Invoke_List(ID) : SurveyedSurfaceService.ListDirect(ID);

                if (ss == null || ss.Count == 0)
                    MessageBox.Show("No surveyed surfaces");
                else
                    MessageBox.Show("Surveyed Surfaces:\n" + ss.Select(x => x.ToString() + "\n").Aggregate((s1, s2) => s1 + s2));
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        /// <summary>
        /// Create direct access
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_1(object sender, EventArgs e)
        {
            SurveyedSurfaceService = new SurveyedSurfaceService(RaptorGrids.RaptorGridName(), RaptorCaches.MutableNonSpatialCacheName());
            SurveyedSurfaceService.Init(null);

            DesignsService = new DesignsService(RaptorGrids.RaptorGridName(), RaptorCaches.MutableNonSpatialCacheName());
            //DesignsService.Init(null);
        }

        private void btnRemoveSurveyedSurface_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!long.TryParse(txtSiteModelID.Text, out long SiteModelID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the site model ID
            if (!long.TryParse(txtSurveyedSurfaceID.Text, out long SurveydSurfaceID))
            {
                MessageBox.Show("Invalid Surveyed Surface ID");
                return;
            }

            // Invoke the service to remove the surveyed surface
            try
            {
                bool result = false;

                if (DeployedSurveyedSurfaceService != null)
                {
                    result = DeployedSurveyedSurfaceService.Invoke_Remove(SiteModelID, SurveydSurfaceID);
                }
                else
                {
                    result = SurveyedSurfaceService.RemoveDirect(SiteModelID, SurveydSurfaceID);
                }

                MessageBox.Show($"Result for removing surveyed surface ID {SurveydSurfaceID} from Site Model {SiteModelID}: {result}");
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void btnRemoveDesign_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!long.TryParse(txtSiteModelID.Text, out long SiteModelID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the design ID
            if (!long.TryParse(txtDesignID.Text, out long DesignID))
            {
                MessageBox.Show("Invalid design ID");
                return;
            }

            // Invoke the service to remove the design
            try
            {
                bool result = SurveyedSurfaceService.RemoveDirect(SiteModelID, DesignID);

                MessageBox.Show($"Result for removing design ID {DesignID} from Site Model {SiteModelID}: {result}");
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void btnListDesigns_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            try
            {
                // Get the site model ID
                if (!long.TryParse(txtSiteModelID.Text, out long ID))
                {
                    MessageBox.Show("Invalid Site Model ID");
                    return;
                }

                VSS.VisionLink.Raptor.Designs.Storage.Designs designList = DesignsService.ListDirect(ID);

                if (designList == null || designList.Count == 0)
                    MessageBox.Show("No designs");
                else
                    MessageBox.Show("Designs:\n" + designList.Select(x => x.ToString() + "\n").Aggregate((s1, s2) => s1 + s2));
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void btnAddAsNewDesign_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!long.TryParse(txtSiteModelID.Text, out long ID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the offset
            if (!double.TryParse(txtOffset.Text, out double offset))
            {
                MessageBox.Show("Invalid design offset");
                return;
            }

            // Invoke the service to add the design
            try
            {
                // Load the file and extract its extents
                TTMDesign TTM = new TTMDesign(SubGridTree.DefaultCellSize);
                TTM.LoadFromFile(Path.Combine(new string[] { txtFilePath.Text, txtFileName.Text }));

                BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
                TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
                TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

                // Create the new design for the site model
                DesignsService.AddDirect(ID,
                                         new DesignDescriptor(Guid.NewGuid().GetHashCode(), "", "", txtFilePath.Text, txtFileName.Text, offset),
                                         extents,
                                         out long DesignID);

                // Store the existence map for the design for later use
                ExistenceMaps.SetExistenceMap(ID, Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, DesignID, TTM.SubgridOverlayIndex());
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }
    }
}
