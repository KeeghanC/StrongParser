using System.Diagnostics;
using System.IO;
using ScottPlot;
namespace Strong_Parser_C3 {
    /*** 
     * TODO
     *  Enumify exercise names
     * 
     * ***/
    public partial class Strong_Parser_Form : Form {
        public Strong_Parser? StrongParser { get; set; }
        public StrongGrapher? StrongGrapher { get; set;}
        public Strong_Parser_Form() {
            InitializeComponent();
        }

        private void choosePerson1ToolStripMenuItem_Click(object sender, EventArgs e) {
            this.StrongParser = new Strong_Parser();
            this.StrongGrapher = new StrongGrapher();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Title = "Select all strong files you would like to parse";
            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "CSV files (*.csv)|*.csv";

            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++) {
                    string filename = openFileDialog1.FileNames[i];
                    StrongParser.StrongFiles.Add(new StrongFile(filename));
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            this.StrongGrapher.AddDataToPlot(this.StrongParser.StrongFiles);
            this.StrongGrapher.showPlot();
        }

        private List<ExerciseEntry> GetMaximumWeightForEachDay(List<ExerciseEntry> exerciseEntries) {
            List<ExerciseEntry> filteredExerciseEntries = new List<ExerciseEntry>();

            for (int i = 0; i < exerciseEntries.Count; i++) {
                bool addToList = true;
                ExerciseEntry entry = exerciseEntries[i];
                // Go through each exercise entry and check to see if this is a new highest weight for that day
                for (int j = 0; j < filteredExerciseEntries.Count; j++) {
                    ExerciseEntry filteredEntry = filteredExerciseEntries[j];
                    if (datesAreOnTheSameDay(filteredExerciseEntries[j].Date, entry.Date)) {
                        if (filteredEntry.Weight < entry.Weight) {
                            filteredExerciseEntries[j] = entry;//.Weight = Math.Max(filteredEntry.Weight, entry.Weight);
                        }
                        addToList = false;
                    }
                }
                if (addToList) filteredExerciseEntries.Add(entry);
            }
            return filteredExerciseEntries;
        }

        private bool datesAreOnTheSameDay(DateTime date1, DateTime date2) {
            if (date1.Day == date2.Day) {
                if (date1.Month == date2.Month) {
                    if (date1.Year == date2.Year) {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ShowBenchPressData() {
            string information = "";
            for (int i = 0; i < StrongParser.StrongFiles.Count; i++) {
                StrongFile strongFile = StrongParser.StrongFiles[i];
                for (int j = 0; j < strongFile.ExerciseEntries.Count; j++) {
                    ExerciseEntry exerciseEntry = strongFile.ExerciseEntries[j];
                    if (exerciseEntry.ExerciseName == "\"Bench Press (Dumbbell)\"") {
                        information += exerciseEntry.Date.ToLongDateString() + " " + exerciseEntry.Weight + "x" + exerciseEntry.Reps + '\n';
                    }
                }
            }
            MessageBox.Show(information);
        }
    }
    public class Strong_Parser {
        public Strong_Parser() {
            this.StrongFiles = new List<StrongFile>();
        }
        public List<StrongFile>? StrongFiles;

        public void DrawExerciseGraph(List<ExerciseEntry> exerciseEntries) {
            List<double> exerciseDates = new List<double>();
            List<double> exerciseWeights = new List<double>();
            List<double> exerciseReps = new List<double>();

            for (int i = 0; i < exerciseEntries.Count; i++) {
                ExerciseEntry exerciseEntry = exerciseEntries[i];
                exerciseDates.Add(exerciseEntry.Date.ToOADate());
                exerciseWeights.Add(exerciseEntry.Weight);
                exerciseReps.Add((double)exerciseEntry.Reps);
            }


            
        }
    }
    public class StrongFile {
        public string FilePath;
        public List<ExerciseEntry>? ExerciseEntries;

        public StrongFile(string filePath) {
            this.FilePath = filePath;
            ExerciseEntries = new List<ExerciseEntry>();
            DecodeFile();
        }

        public void DecodeFile() {
            // Open file path as a file object
            FileStream file = new FileStream(FilePath, FileMode.Open);

            List<string[]> lines = new List<string[]>();

            using (StreamReader reader = new StreamReader(file)) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    string[] fields = line.Split(',');
                    lines.Add(fields);
                }
            }
            file.Close();

            // Skip first header line
            for (int i = 1; i < lines.Count; i++) {
                ExerciseEntry exerciseEntry = new ExerciseEntry();

                exerciseEntry.Date = DateTime.Parse(lines[i][0]);
                exerciseEntry.WorkoutName = lines[i][1];
                exerciseEntry.Duration = lines[i][2];
                exerciseEntry.ExerciseName = lines[i][3];
                exerciseEntry.SetNumber = lines[i][4];
                exerciseEntry.Weight = float.Parse(lines[i][5]);
                exerciseEntry.Reps = int.Parse(lines[i][6]);
                exerciseEntry.Distance = float.Parse(lines[i][7]);
                exerciseEntry.Seconds = float.Parse(lines[i][8]);
                exerciseEntry.Notes = lines[i][9];
                exerciseEntry.WorkoutNotes = lines[i][10];
                exerciseEntry.RPE = ((lines[i][11]) == "") ? 0 : float.Parse(lines[i][11]);

                this.ExerciseEntries.Add(exerciseEntry);
            }
            for (int i = 0; i < this.ExerciseEntries.Count; i++) {
                Console.WriteLine(i.ToString());
            }
        }

        public List<ExerciseEntry> FilterEntriesByName(string name) {
            List<ExerciseEntry> entries = new List<ExerciseEntry>();
            for (int i = 0; i < this.ExerciseEntries.Count; i++) {
                ExerciseEntry entry = this.ExerciseEntries[i];
                if (entry.ExerciseName == name) {
                    entries.Add(entry);
                }
            }
            return entries;
        }
    }

    public class ExerciseEntry {
        public DateTime Date;
        public string? WorkoutName;
        public string? Duration;
        public string? ExerciseName;
        public string? SetNumber;
        public float Weight;
        public int Reps;
        public float Distance;
        public float Seconds;
        public string? Notes;
        public string? WorkoutNotes;
        public float RPE;
    }


    public class StrongGrapher {
        List<Color> colors; // Currently only supports 2 colours -- Todo make this 10 
        Plot plot;
        string imagePath;
        public StrongGrapher() {
            initPlot();
            colors = new List<Color>();
            colors.Add(Color.Red);
            colors.Add(Color.Purple);

            this.imagePath = "GeneratedImage.jpg";
        }
        public void initPlot() {
            this.plot = new Plot(10000, 1080);

            // Set the axis labels
            plot.XLabel("Date");
            plot.YLabel("Weight");

            // Set the axis date format
            plot.XAxis.DateTimeFormat(true);

            // Draw the legend on the chart
            plot.Legend();
        }

        public void showPlot() {
            Image bitmap = plot.GetBitmap();
            bitmap.Save(imagePath);

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(imagePath) {
                UseShellExecute = true
            };

            p.Start();
        }

        public void AddDataToPlot(List<StrongFile> strongFiles) {
            for (int i = 0; i < strongFiles.Count; i++) {
                List<ExerciseEntry> exerciseEntries = strongFiles[i].FilterEntriesByName("\"Bench Press (Dumbbell)\"");
                exerciseEntries = GetMaximumWeightForEachDay(exerciseEntries);

                List<double> exerciseDates = new List<double>();
                List<double> exerciseWeights = new List<double>();
                List<double> exerciseReps = new List<double>();

                for (int j = 0; j < exerciseEntries.Count; j++) {
                    ExerciseEntry exerciseEntry = exerciseEntries[j];
                    exerciseDates.Add(exerciseEntry.Date.ToOADate());
                    exerciseWeights.Add(exerciseEntry.Weight);
                    exerciseReps.Add((double)exerciseEntry.Reps);
                }

                this.plot.PlotScatter(exerciseDates.ToArray(), exerciseWeights.ToArray(), colors[i], 1, 5, strongFiles[i].FilePath);

                for (int j = 0; j < exerciseWeights.Count; j++) plot.AddText(exerciseWeights[j].ToString() + "x" + exerciseReps[j].ToString(), exerciseDates[j], exerciseWeights[j], color: colors[i]);
            }
        }

        private List<ExerciseEntry> GetMaximumWeightForEachDay(List<ExerciseEntry> exerciseEntries) {
            List<ExerciseEntry> filteredExerciseEntries = new List<ExerciseEntry>();

            for (int i = 0; i < exerciseEntries.Count; i++) {
                bool addToList = true;
                ExerciseEntry entry = exerciseEntries[i];
                // Go through each exercise entry and check to see if this is a new highest weight for that day
                for (int j = 0; j < filteredExerciseEntries.Count; j++) {
                    ExerciseEntry filteredEntry = filteredExerciseEntries[j];
                    if (datesAreOnTheSameDay(filteredExerciseEntries[j].Date, entry.Date)) {
                        if (filteredEntry.Weight < entry.Weight) {
                            filteredExerciseEntries[j] = entry;//.Weight = Math.Max(filteredEntry.Weight, entry.Weight);
                        }
                        addToList = false;
                    }
                }
                if (addToList) filteredExerciseEntries.Add(entry);
            }
            return filteredExerciseEntries;
        }
        private bool datesAreOnTheSameDay(DateTime date1, DateTime date2) {
            if (date1.Day == date2.Day) {
                if (date1.Month == date2.Month) {
                    if (date1.Year == date2.Year) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}