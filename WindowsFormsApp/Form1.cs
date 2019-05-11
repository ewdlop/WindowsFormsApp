using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class Form1 : Form
    {

        SqlConnection cnn;

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void Xls_Click(object sender, EventArgs e)
        {
            string promptValue = UserInputDialog("Please enter number of lines per cut:", "Hello");

            if (promptValue != "Bad Input")
            {
                string sumbmissionFilePath;
                int cuts = int.Parse(promptValue);

                OpenFileDialog openSubmissionXlsFileDialog = new OpenFileDialog
                {
                    Title = "Open Execl File",
                    Filter = "Excel Files|*.xls",
                    InitialDirectory = @"D:\AspNet\WindowsFormsApp\New folder\"
                };

                OpenFileDialog openSubmissionXlsmFileTenmplateDialog = new OpenFileDialog
                {
                    Title = "Open Execl File",
                    Filter = "Excel Files|*.xlsm",
                    InitialDirectory = @"D:\AspNet\WindowsFormsApp\New folder\"
                };

                if (openSubmissionXlsFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileStream submissionFile = (FileStream)openSubmissionXlsFileDialog.OpenFile();
                        sumbmissionFilePath = submissionFile.Name;
                        HSSFWorkbook xlsWorkBook = new HSSFWorkbook(submissionFile);
                        ISheet submissionSourceSheet1 = xlsWorkBook.GetSheetAt(0);
                        int totalLines = submissionSourceSheet1.LastRowNum;
                        this.copyProgressBar.Maximum = totalLines;
                        int numberOfCuts = totalLines / cuts + 1;
                        submissionFile.Close();
                        if (openSubmissionXlsmFileTenmplateDialog.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                int startingRow = 1;
                                int endingRow = Math.Min(cuts, totalLines);
                                int partCount = 1;
                                FileStream submissionpPartTemplate = (FileStream)openSubmissionXlsmFileTenmplateDialog.OpenFile();

                                for (partCount = 1; partCount <= numberOfCuts; partCount++)
                                {
                                    this.copyStatusLabel.Text = "Copying Part " + partCount;
                                    this.copyStatusLabel.Refresh();
                                    CutsAndMakeCopyies(sumbmissionFilePath, submissionpPartTemplate, partCount, startingRow, endingRow);
                                    //await CutsAndMakeCopyies(sumbmissionFilePath, submissionpPartTemplate, partCount, startingRow, endingRow);
                                    startingRow += cuts;

                                    if (partCount + 1 == numberOfCuts)
                                        endingRow = totalLines;
                                    else
                                        endingRow += cuts;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Xlsm file openinig issue. Error: " + ex.Message + " " + ex.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Xls file openinig issue. Error: " + ex.Message + " " + ex.ToString());
                    }
                }
                this.copyStatusLabel.Text = "test";
                Process.Start(@"D:\AspNet\WindowsFormsApp\New folder\");
            }
        }

        private void CutsAndMakeCopyies(String sumbmissionFilePath, FileStream submissionTemplate, int partCount, int startingRow, int endingRow )
        {
            int xlsmRowOfffset = 5;
            int xlsmColOffset = 1;

            try
            {
                FileStream newXlsm = new FileStream(@"D:\AspNet\WindowsFormsApp\New folder\output-p" + partCount + ".xlsm", FileMode.Create, FileAccess.ReadWrite);
                //await submissionTemplate.CopyToAsync(newXlsm);
                submissionTemplate.CopyTo(newXlsm);
                submissionTemplate.Position = 0;
                newXlsm.Close();
                try
                {
                    newXlsm = new FileStream(@"D:\AspNet\WindowsFormsApp\New folder\output-p" + partCount + ".xlsm", FileMode.Open, FileAccess.ReadWrite);
                    XSSFWorkbook xlsmWorkBook = new XSSFWorkbook(newXlsm);
                    newXlsm.Close();
                    FileStream sumbmissionXlsFile = new FileStream(sumbmissionFilePath, FileMode.Open, FileAccess.Read);
                    HSSFWorkbook xlsWorkBook = new HSSFWorkbook(sumbmissionXlsFile);
                    ISheet newXlsmSheet1 = xlsmWorkBook.GetSheet("Garden & Patio");
                    ISheet submissionSourceSheet1 = xlsWorkBook.GetSheetAt(0);
                    int lastSumbissionCellNum = submissionSourceSheet1.GetRow(1).LastCellNum;
                    newXlsm = new FileStream(@"D:\AspNet\WindowsFormsApp\New folder\output-p" + partCount + ".xlsm", FileMode.Create, FileAccess.Write);
                    try
                    {
                        for (int submissionRow = startingRow; submissionRow <= endingRow; submissionRow++)
                        {
                            UpdateProgressBar();
                            for (int submissionCol = 1; submissionCol <= lastSumbissionCellNum; submissionCol++)
                            {
                                if (submissionSourceSheet1.GetRow(submissionRow).GetCell(submissionCol) != null)
                                {
                                    ICell copyingCell = submissionSourceSheet1.GetRow(submissionRow).GetCell(submissionCol);
                                    IRow pastingRow = newXlsmSheet1.GetRow(submissionRow - startingRow + xlsmRowOfffset + 1) ?? newXlsmSheet1.CreateRow(submissionRow - startingRow + xlsmRowOfffset + 1);
                                    ICell pastingCell = pastingRow.GetCell(submissionCol + xlsmColOffset) ?? pastingRow.CreateCell(submissionCol + xlsmColOffset);
                                    switch (copyingCell.CellType)
                                    {
                                        case CellType.Boolean:
                                            pastingCell.SetCellValue(copyingCell.BooleanCellValue);
                                            break;
                                        case CellType.Numeric:
                                            pastingCell.SetCellValue(copyingCell.NumericCellValue);
                                            break;
                                        case CellType.String:
                                            pastingCell.SetCellValue(copyingCell.StringCellValue);
                                            break;
                                    }
                                    
                                }
                            }
                        }
                        xlsmWorkBook.Write(newXlsm);
                        newXlsm.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Copying rows to new xlsm file issue. Error: " + ex.Message + " " + ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Copied files opening issue D. Error: " + ex.Message + " " + ex.ToString());
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("Somethign went wrong with copying the xlsm. Error: " + ex.Message + " " + ex.ToString());
            }
        }

        private void UpdateProgressBar()
        {
            this.copyProgressBar.PerformStep();
            this.copyProgressBar.Refresh();
        }
      

        public string UserInputDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text, Width = 200 };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 200 };
            Button OK = new Button() { Text = "Ok", Left = 100, Width = 100, Top = 80, DialogResult = DialogResult.None};

            
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(OK);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = OK;


            OK.Click += (sender, e) => {

                Match match = Regex.Match(textBox.Text, @"^[1-9][0-9]*$");
                if (match.Success)
                {
                    prompt.DialogResult = DialogResult.OK;
                    prompt.Close();
                }
                else
                {
                    MessageBox.Show("Something wrong with the input");
                }
            };

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "Bad Input";
        }

        private void SelectDirectory(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string path = fbd.SelectedPath;
                    this.directoryTextBox.Text = path;
                }
            }
        }

        private void OpenDirectory(object sender, EventArgs e)
        {
            try
            {
                if(directoryTextBox.TextLength > 0)
                {
                    Process.Start(directoryTextBox.Text);
                }
            
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadFromLocalDataBase(object sender, EventArgs e)
        {
            string connetionString = ConfigurationManager.ConnectionStrings["WindowsFormsApp.Properties.Settings.masterConnectionString"].ConnectionString;

            try
            {
                cnn = new SqlConnection(connetionString);
                cnn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.PurchaseOrderDetail", cnn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dataGridView1.DataSource = ds.Tables[0];
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cnn.Close(); 
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void purchaseOrderDetailBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.purchaseOrderDetailBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.masterDataSet1);

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'masterDataSet1.PurchaseOrderDetail' table. You can move, or remove it, as needed.
            this.purchaseOrderDetailTableAdapter.Fill(this.masterDataSet1.PurchaseOrderDetail);

        }
    }

}
