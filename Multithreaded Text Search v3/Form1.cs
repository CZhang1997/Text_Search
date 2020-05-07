using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

/**
 * @Author: Churong Zhang
 * @Email: churong.zhang@utdallas.edu
 * @Date:  4/10/2020
 * This is the main class of the from, it use 50 workers to search through a file
 */


namespace Multithreaded_Text_Search
{
    public partial class Form1 : Form
    {
        string selectedFileName;    // contain the absolute path of the file
        string keyword;             // the key word to search for
        private bool searchOn;
        private Queue<Text> queue;
        private BackgroundWorker loadWorker;
        // lock it such only that only one worker can modify the list view
        public Form1()
        {
            InitializeComponent();
            // define varibales 
            selectedFileName = "";
            // set the list view sort by acending by line number 
            ResultView.ListViewItemSorter = new ListViewItemCompare();
            // center the form
            this.CenterToScreen();
            // make the window in maximized state
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            showMessage("You can save the result of the text by clicking the row of the text on the result list to clipboard after search completed");
            searchOn = false;
            Clipboard.SetText("you dumb");
        }
        private void addLineToListView(long lineNumber, string lineText)
        {   // create a item with line number as column 1, line text as column 2
            ListViewItem item = new ListViewItem("" + lineNumber);
            item.SubItems.Add(lineText);
            ResultView.Items.Add(item);

        }
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            // select a file from open file dialog
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text files (*.txt)|*.txt";
            openFile.FilterIndex = 0;
            openFile.RestoreDirectory = true;
            removeMessage();
            if (openFile.ShowDialog() == DialogResult.OK)
            {   // if ok then we get the path of the file name
                selectedFileName = openFile.FileName;
                filePathTextBox.Text = selectedFileName;
            }
            else
            {   // show warning message 
                showMessage("No file was selected");
            }
        }


        private void searchButton_Click(object sender, EventArgs e)
        {
            if (searchOn)
            {
                loadWorker.CancelAsync();
                return;
            }
            // remove the Items from the previous search if exist
            ResultView.Items.Clear();
            // get the selected file name;
            selectedFileName = filePathTextBox.Text;
            removeMessage();        // remove message if there is any
            if (File.Exists(selectedFileName))  // check if the file exist
            {
                keyword = keywordTextBox.Text;  // get the key word to search for
                if (keyword.Length == 0)    // if the keyword is empty then show a message and return
                {
                    showMessage("Keyword can not be empty.");
                    return;

                }
                filePathTextBox.Enabled = false;
                BrowseButton.Enabled = false;
                keywordTextBox.Enabled = false;
                caseCheckBox.Enabled = false;
                searchOn = true;
                searchButton.Text = "Cancel";
                // creaete the worker
                loadWorker = new BackgroundWorker();
                loadWorker.WorkerSupportsCancellation = true;
                loadWorker.WorkerReportsProgress = true;
                loadWorker.ProgressChanged += loadWorkerReportProcess;
                loadWorker.RunWorkerCompleted += loadWorkerRunWorkerCompleted;
                loadWorker.DoWork += loadWorkerDoWork;
                loadWorker.RunWorkerAsync();

            }
            else
            {   // show error message to user 
                showMessage("File Does not exist! please try again");
            }
        }

        private void fixPositions() // fix each component's location base on the size of the screen
        {   
            // define varibales
            int formWidth = this.Width;
            int formHeight = this.Height;
            int leftAlight = 30;    // space to the left size
            int rightAlight = 60;   // space to the right size
            int filePathY = 20;     // y axis for the file selected row from the ui
            int keyWordY = filePathY + 40;  // y axis for key word row from ui 
            int messageY = keyWordY + 40;   // y axis to where the message label locate
            int progressY = messageY + 20;
            int resultViewY = progressY + 40;    // y axist for the list view
            int spaceBetween = 30;          // the space bewtween two components

            // set the location for the two label
            filePathLabel.Location = new Point(leftAlight, filePathY);
            keywordLabel.Location = new Point(leftAlight, keyWordY);

            // set the location for the button
            BrowseButton.Location = new Point(formWidth - rightAlight - BrowseButton.Width, filePathY);
            searchButton.Location = new Point(formWidth - rightAlight - searchButton.Width, keyWordY);

            // set the loaction for the two text box
            filePathTextBox.Location = new Point(leftAlight + filePathLabel.Width + spaceBetween, filePathY);
            keywordTextBox.Location = new Point(leftAlight + keywordLabel.Width + spaceBetween, keyWordY);

            caseCheckBox.Location = new Point(searchButton.Location.X - caseCheckBox.Width, keyWordY);
            // change the text box width base on the width of the screen
            filePathTextBox.Width = formWidth - rightAlight - BrowseButton.Width - spaceBetween * 2 - leftAlight - filePathLabel.Width;
            keywordTextBox.Width = formWidth - rightAlight - searchButton.Width - spaceBetween * 2 - leftAlight - keywordLabel.Width - caseCheckBox.Width;

            // set the location for the message label at the center
            messageLabel.Location = new Point(formWidth / 2 - messageLabel.Width / 2, messageY);

            // set up the properties of the progress bar
            progressBar.Location = new Point(leftAlight, progressY);
            progressBar.Width = formWidth - leftAlight - rightAlight;

            // Set up the list view and its attributes
            ResultView.Location = new Point(leftAlight, resultViewY);
            ResultView.Width = formWidth - rightAlight - leftAlight;
            ResultView.Height = formHeight - resultViewY - spaceBetween* 2;
            int lineWidth = 100;    // change the width of the columns
            ResultView.Columns[0].Width = lineWidth;
            ResultView.Columns[1].Width = ResultView.Width - lineWidth;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {   // fix the position each time the form size changed
            fixPositions();
        }
        private void showMessage(string message)
        {   // show a message to the user and center the message
            messageLabel.Text = message;
            messageLabel.Location = new Point(this.Width / 2 - messageLabel.Width / 2, messageLabel.Location.Y);
        }
        private void removeMessage()
        {   // remove the message
            messageLabel.Text = "";
        }

        // worker functions
        private void loadWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            // create a queue to save line that should be added to the UI
            queue = new Queue<Text>();
            // get the file length
            FileInfo file = new FileInfo(selectedFileName);
            long fileLength = file.Length;  // this size is sightly larger than the number of char in the file
            // create the reader
            StreamReader reader = new StreamReader(selectedFileName);
            if(!caseCheckBox.Checked)
            {
                keyword = keyword.ToLower();
            }
            
            long charRead = 0;      // keep the count of how many char has been read
            long lineNumber = 1;    // the line number where the line was read
            string line;            // the line text
;            while ((line = reader.ReadLine()) != null)
            {
                charRead += line.Length;    // update how many char was read
                int precent = (int)(((double)charRead) * 100.0 / fileLength);   // count how many percent has done
                if(loadWorker.CancellationPending)
                {   // if cancel then stop this function
                    e.Cancel = true;
                    reader.Close();
                    Console.WriteLine("Load worker request to cancel");
                    return;
                }
                // check if this line contain the key words
                string line2 = line;
                if (!caseCheckBox.Checked)
                {
                    line2 = line2.ToLower();
                }
                if(line2.Contains(keyword))
                {   // if is, then add it to the queue
                    queue.Enqueue(new Text(lineNumber, line));
                }
                loadWorker.ReportProgress(precent); // report the progress
                lineNumber++;   // increment the line number
                System.Threading.Thread.Sleep(1);   // pause one mili second
            }
            reader.Close(); // close the file
        }
        private void loadWorkerReportProcess(object sender, ProgressChangedEventArgs e)
        {
            showMessage("Successfully scanned " + e.ProgressPercentage + "% of the file!"); // show the message
            progressBar.Value = e.ProgressPercentage;   // change the progressBar to its value
            while (queue.Count != 0)
            {
                Text text = queue.Dequeue();    // add every line in the queue to the list view 
                addLineToListView(text.getLineNumber(), text.getText());
            }
        }
        private void loadWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {   // worker finish end get the result
            if(e.Cancelled)
            {
                showMessage(messageLabel.Text + " -> Search Cancel");
            }
            else
            {
                showMessage("Scanned Finish!");
                progressBar.Value = 100;
            }
            searchOn = false;
            searchButton.Text = "Search";
            filePathTextBox.Enabled = true;
            BrowseButton.Enabled = true;
            keywordTextBox.Enabled = true;
            caseCheckBox.Enabled = true;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ResultView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(ResultView.SelectedItems.Count > 0)  // if there was a row selected
            {
                string lineNumber = ResultView.SelectedItems[0].Text;
                string line = ResultView.SelectedItems[0].SubItems[1].Text;
                Clipboard.SetText(line);
                showMessage("The text at line " + lineNumber + " has been save to Clipboard");
            }
        }
    }
}
