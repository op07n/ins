﻿using InertialNavigationSystem_Demo.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.WinForms;
using LiveCharts.Defaults;
using System.Windows.Media;
using ZedGraph;
using InertialNavigationSystem;

namespace InertialNavigationSystem_Demo
{
    public partial class Form1 : Form
    {

        CSVFile csv;

        PointPairList list1 = new PointPairList();
        PointPairList list2 = new PointPairList();
        LineItem myCurve;


        public Form1()
        {
            InitializeComponent();
            CreateGraph(Chart1);
            FilterSelector.SelectedIndex = 0;
        }

        private void CreateGraph(ZedGraphControl zgc)
        {
            // get a reference to the GraphPane
            GraphPane myPane = zgc.GraphPane;

            // Set the Titles
            myPane.Title.Text = "Signal Processing Demo";
            myPane.XAxis.Title.Text = "t [s]";
            myPane.YAxis.Title.Text = "Value";

            myCurve = zgc.GraphPane.AddCurve("Signal", list1, System.Drawing.Color.Blue, SymbolType.None);
            myCurve = zgc.GraphPane.AddCurve("Signal integral", list2, System.Drawing.Color.Red, SymbolType.None);

            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

        }

        private void GenerateChart()
        {
            if (csv == null)
                return;

            WaitingScreen.Visible = true;
            this.Refresh();

            list1.Clear();
            list2.Clear();

            IFilter Filter;

            string FilterType = FilterSelector.Text;

            switch (FilterType)
            {
                case "FIR Filter":
                    Filter = new FIRFilter(new List<double>() { 0.25, 0.25, 0.25, 0.25 });
                    break;
                case "Alpha Beta Filter":
                    Filter = new AlphaBetaFilter(0.2, 0.3);
                    break;
                case "Smart Alpha Beta Filter":
                    Filter = new SmartAlphaBetaFilter(0.003307643036326, 200);
                    break;
                default:
                    Filter = null;
                    break;
            }

            int PreviousProgress = 0;
            int Progress = 0;
            int i = 0;


            Integrator integrator = new Integrator();
            
            foreach (KeyValuePair<double, List<double>> entry in csv.Data)
            {
                InertialNavigationSystem.Sample sample = new InertialNavigationSystem.Sample(entry.Key, entry.Value[ColumnSelector.SelectedIndex]);

                if (Filter != null)
                {
                    sample = Filter.AddSample(sample);
                }
                
                list1.Add(sample.Time, sample.Value);

                integrator.AddSample(sample);
                if(DrawIntegral.Checked)
                    list2.Add(sample.Time, integrator.Value);

                i++;

                Progress = i * 100 / csv.Data.Count;

                if(PreviousProgress!=Progress)
                {
                    ProgressIndicator.Value = Progress;
                    Application.DoEvents();
                }

                PreviousProgress = Progress;

            }

            Chart1.AxisChange();
            Chart1.Refresh();

            WaitingScreen.Visible = false;

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void importDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                try
                {
                    csv = new CSVFile(openFileDialog.FileName, ';');
                    try
                    {
                        ColumnSelector.Items.Clear();
                        for(int i=0; i<csv.Data.First().Value.Count; i++)
                        {
                            ColumnSelector.Items.Add((i+1).ToString());
                        }
                        ColumnSelector.SelectedIndex = 0;
                        //GenerateChart();
                    }
                    catch(Exception ex)
                    {
                        throw new Exception("Chart generation error");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void FilterSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateChart();
        }

        private void fIRFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void alphaBetaFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void smartAlphaBetaFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ColumnSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateChart();
        }

        private void toolStripMenuItem2_CheckedChanged(object sender, EventArgs e)
        {
            GenerateChart();
        }
    }
}
