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
        Search search;  // Process the file that was selected
        string selectedFileName;    // contain the absolute path of the file
        string keyword;             // the key word to search for
        private bool searchOn;
        private Queue<Text> queue;
        private BackgroundWorker loadWorker;
        private BackgroundWorker processWorker;
        // lock it such only that only one worker can modify the list view
        private readonly object viewLock = new object();    
        public Form1()
        {
            InitializeComponent();
            // define varibales 
            search = new Search();
            selectedFileName = "";
            // set the list view sort by acending by line number
            ResultView.ListViewItemSorter = new ListViewItemCompare();
            // center the form
            this.CenterToScreen();
            // make the window in maximized state
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            removeMessage(); // remove message label
            searchOn = false;
        }
        private void addLineToListView(long lineNumber, string lineText)
        {   // create a item with line number as column 1, line text as column 2
            ListViewItem item = new ListViewItem("" + lineNumber);
            item.SubItems.Add(lineText);
            if (ResultView.InvokeRequired)
            {
                ResultView.Invoke((MethodInvoker)delegate ()
                {
                    ResultView.Items.Add(item);
                });
            }

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
                keyword = keywordTextBox.Text.ToLower();  // get the key word to search for
                if (keyword.Length == 0)    // if the keyword is empty then show a message and return
                {
                    showMessage("Keyword can not be empty.");
                    return;

                }
                filePathTextBox.Enabled = false;
                BrowseButton.Enabled = false;
                keywordTextBox.Enabled = false;
                searchOn = true;
                searchButton.Text = "Cancel";
                // creaete a worker that read the lines into a queue
                loadWorker = new BackgroundWorker();
                loadWorker.WorkerSupportsCancellation = true;
                loadWorker.WorkerReportsProgress = true;
                loadWorker.RunWorkerCompleted += loadWorkerRunWorkerCompleted;
                loadWorker.DoWork += loadWorkerDoWork;
                //string[] argv = { selectedFileName, keyword };
                processWorker = new BackgroundWorker();
                processWorker.WorkerSupportsCancellation = true;
                processWorker.WorkerReportsProgress = true;
                processWorker.RunWorkerCompleted += processWorkerRunWorkerCompleted;
                processWorker.DoWork += processWorkerDoWork;

                loadWorker.RunWorkerAsync();
                processWorker.RunWorkerAsync();

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
            int resultViewY = messageY + 20;    // y axist for the list view
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

            // change the text box width base on the width of the screen
            filePathTextBox.Width = formWidth - rightAlight - BrowseButton.Width - spaceBetween * 2 - leftAlight - filePathLabel.Width;
            keywordTextBox.Width = formWidth - rightAlight - searchButton.Width - spaceBetween * 2 - leftAlight - keywordLabel.Width;

            // set the location for the message label at the center
            messageLabel.Location = new Point(formWidth / 2 - messageLabel.Width / 2, messageY);

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
            queue = new Queue<Text>();
            StreamReader reader = new StreamReader(selectedFileName);
            keyword = keyword.ToLower();
            long lineNumber = 1;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if(loadWorker.CancellationPending)
                {
                    e.Cancel = true;
                    reader.Close();
                    Console.WriteLine("Load worker request to cancel");
                    return;
                }
                queue.Enqueue(new Text(lineNumber, line));
                lineNumber++;
                System.Threading.Thread.Sleep(1);
            }
            reader.Close();
        }
        private void loadWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {   // worker finish end get the result
            if(e.Cancelled)
            {
                showMessage("Cancel Successfully");
            }
            processWorker.CancelAsync();

        }

        private void processWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            //string[] argv = (string[])e.Argument;
            //string filename = argv[0];
            //string key = argv[1];
            while (true)
            {
                if(queue.Count != 0)
                {
                    Text text = queue.Dequeue();
                    string line = text.getText().ToLower();
                    if (line.Contains(keyword))
                    {
                        addLineToListView(text.getLineNumber(), text.getText());
                    }
                }
                if(processWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        private void processWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {   // worker finish end get the result
            searchOn = false;
            searchButton.Text = "Search";
            filePathTextBox.Enabled = true;
            BrowseButton.Enabled = true;
            keywordTextBox.Enabled = true;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
