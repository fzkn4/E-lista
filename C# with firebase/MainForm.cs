using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C__with_firebase;
using C__with_firebase.Models; 
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Firebase.Database; 
using Firebase.Database.Query;

namespace C__with_firebase
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            populationFilter.Text = "All";
            //InitializingSwitchEventArgs Charts
            SetupStaticDoughnutChart();
            SetupStaticSplineAreaChart();

            hide_panels();
            homePanel.Visible = true;
            navHome.Checked = true;

            // gender checkboxes
            male.CheckedChanged += HandleGenderCheckBoxToggle;
            female.CheckedChanged += HandleGenderCheckBoxToggle;
            // status checkboxes
            single.CheckedChanged += HandleStatusCheckBoxToggle;
            married.CheckedChanged += HandleStatusCheckBoxToggle;
            divorced.CheckedChanged += HandleStatusCheckBoxToggle;
            widowed.CheckedChanged += HandleStatusCheckBoxToggle;
            other.CheckedChanged += HandleStatusCheckBoxToggle;

            _ = LoadPopulationCounts();


            populationFilter.Items.Clear(); 
            populationFilter.Items.Add("All");
            populationFilter.Items.Add("Male");
            populationFilter.Items.Add("Female");
            populationFilter.SelectedIndex = 0;

            _ = LoadPopulationCountByDateChart();
            _ = LoadGenderDistributionChart();
            _ = LoadPopulationTableData();
        }


        private void logout_click(object sender, EventArgs e)
        {
            try
            {
                FirebaseAuthenticationService.Instance.SignOut();
                MessageBox.Show("You have been successfully logged out.", "Signed Out", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during logout: {ex.Message}", "Logout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
                this.Close();
            }
        }

        private void navHome_Click(object sender, EventArgs e)
        {
            ToggleNavButtonCheckedState(sender);
        }

        private void SetupStaticSplineAreaChart()
        {
            splineDataset.DataPoints.Clear();
            splineDataset.Label = "Population Added";
            splineDataset.FillColor = Color.FromArgb(120, 238, 27, 104);
            splineDataset.BorderColor = Color.FromArgb(238, 27, 104);

            if (!splineChart.Datasets.Contains(splineDataset))
            {
                splineChart.Datasets.Add(splineDataset);
            }

            splineChart.Update();
        }

        private void SetupStaticDoughnutChart()
        {
            doughnutDataset.DataPoints.Clear();

            doughnutDataset.DataPoints.Add("Red", 30);
            doughnutDataset.DataPoints.Add("Blue", 50);
            doughnutDataset.DataPoints.Add("Green", 20);
            doughnutDataset.DataPoints.Add("Yellow", 40);

            if (!doughnutChart.Datasets.Contains(doughnutDataset))
            {
                doughnutChart.Datasets.Add(doughnutDataset);
            }
            doughnutChart.Title.Text = "Population Breakdown";
            doughnutChart.Title.Display = true; // Show the title

            doughnutChart.Legend.Position = Guna.Charts.WinForms.LegendPosition.Right;
            doughnutChart.Legend.Display = true;

            doughnutChart.XAxes.Display = false;
            doughnutChart.YAxes.Display = false;


            doughnutChart.Update();
        }

        private void navAdd_Click(object sender, EventArgs e)
        {
            ToggleNavButtonCheckedState(sender);
        }

        private void ToggleNavButtonCheckedState(object clickedButton)
        {

            Guna2Button[] navigationButtons = new Guna2Button[]
            {
                navHome,
                navAdd,
            };

            foreach (Guna2Button button in navigationButtons)
            {
                button.Checked = (button == clickedButton);
            }

            hide_panels();

            if (clickedButton == navHome)
            {
                homePanel.Visible = true;
            }
            else if (clickedButton == navAdd)
            {
                addPanel.Visible = true;
            }
        }
        private void hide_panels()
        {
            homePanel.Visible = false;
            addPanel.Visible = false;
        }

        private void HandleGenderCheckBoxToggle(object sender, EventArgs e)
        {
            Guna2CheckBox clickedCheckBox = sender as Guna2CheckBox;

            if (clickedCheckBox != null && clickedCheckBox.Checked)
            {
                if (clickedCheckBox == male)
                {
                    female.Checked = false;
                }
                else if (clickedCheckBox == female)
                {
                    male.Checked = false;
                }
            }
        }

        private void HandleStatusCheckBoxToggle(object sender, EventArgs e)
        {
            Guna2CheckBox clickedCheckBox = sender as Guna2CheckBox;

            Guna2CheckBox[] statusCheckBoxes = new Guna2CheckBox[]
            {
                single,
                married,
                divorced,
                widowed,
                other
            };

            if (clickedCheckBox != null && clickedCheckBox.Checked)
            {
                foreach (Guna2CheckBox checkBox in statusCheckBoxes)
                {
                    if (checkBox != clickedCheckBox)
                    {
                        checkBox.Checked = false;
                    }
                }
            }
        }

        private async void add_Click(object sender, EventArgs e)
        {
            // ... (Your existing add_Click logic) ...

            // 1. Input Validation
            if (string.IsNullOrWhiteSpace(fname.Text) ||
                string.IsNullOrWhiteSpace(lname.Text) ||
                string.IsNullOrWhiteSpace(birthPlace.Text) ||
                string.IsNullOrWhiteSpace(barangayAddr.Text) ||
                string.IsNullOrWhiteSpace(cityAddr.Text) ||
                string.IsNullOrWhiteSpace(provinceAddr.Text))
            {
                MessageBox.Show("Please fill in all required fields (First Name, Last Name, Birth Place, and Addresses).", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get selected Gender
            string selectedGender = null;
            if (male.Checked) selectedGender = "Male";
            else if (female.Checked) selectedGender = "Female";

            if (selectedGender == null)
            {
                MessageBox.Show("Please select a gender.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Get selected Marital Status
            string selectedStatus = null;
            if (single.Checked) selectedStatus = "Single";
            else if (married.Checked) selectedStatus = "Married";
            else if (divorced.Checked) selectedStatus = "Divorced";
            else if (widowed.Checked) selectedStatus = "Widowed";
            else if (other.Checked) selectedStatus = "Other";

            if (selectedStatus == null)
            {
                MessageBox.Show("Please select a marital status.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4. Get Birth Date from DateTimePicker and convert to Unix Timestamp
            DateTime birthDateTime = birthDate.Value;
            long unixTimestamp = ((DateTimeOffset)birthDateTime).ToUnixTimeSeconds();

            // 5. Create the data object using the PersonData class
            var personData = new PersonData
            {
                FirstName = fname.Text.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(mname.Text) ? null : mname.Text.Trim(),
                LastName = lname.Text.Trim(),
                BirthPlace = birthPlace.Text.Trim(),
                BarangayAddress = barangayAddr.Text.Trim(),
                CityAddress = cityAddr.Text.Trim(),
                ProvinceAddress = provinceAddr.Text.Trim(),
                Gender = selectedGender,
                MaritalStatus = selectedStatus,
                BirthDateTimestamp = unixTimestamp,
                AddedByUserId = FirebaseAuthenticationService.Instance.IsUserLoggedIn() ? FirebaseAuthenticationService.Instance.GetCurrentUser().Uid : null,
                AddedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // 6. Save data to Firebase Realtime Database
            try
            {
                var firebaseDbClient = FirebaseAuthenticationService.Instance.GetFirebaseDatabaseClient();

                var result = await firebaseDbClient
                    .Child("people")
                    .PostAsync(personData);

                Debug.WriteLine($"Data added to Firebase with key: {result.Key}");
                MessageBox.Show("Person data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 7. Clear input fields after successful save
                ClearInputFields();

                // 8. Refresh population counts and chart after adding new data
                _ = LoadPopulationCountByDateChart();
                _ = LoadGenderDistributionChart();
                _ = LoadPopulationTableData();

            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"You must be logged in to save data. Please log in again. Error: {ex.Message}", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Firebase.Database.FirebaseException fex)
            {
                MessageBox.Show($"Firebase Database Error: {fex.Message}\nCheck your database rules and network connection.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while saving data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearInputFields()
        {
            fname.Clear();
            mname.Clear();
            lname.Clear();
            birthPlace.Clear();
            barangayAddr.Clear();
            cityAddr.Clear();
            provinceAddr.Clear();

            // Uncheck all gender checkboxes
            male.Checked = false;
            female.Checked = false;

            // Uncheck all status checkboxes
            single.Checked = false;
            married.Checked = false;
            divorced.Checked = false;
            widowed.Checked = false;
            other.Checked = false;

            // Reset DateTimePicker to current date or a default
            birthDate.Value = DateTime.Now; // Or set to a specific default date like DateTime(1990, 1, 1)
        }


        private async Task LoadPopulationCounts()
        {
            int male = 0;
            int female = 0;
            int total = 0;

            try
            {
                // Get the Firebase Database client from the service
                // This will throw InvalidOperationException if the user is not logged in
                var firebaseDbClient = FirebaseAuthenticationService.Instance.GetFirebaseDatabaseClient();

                // Retrieve all data from the "people" node
                // OnceAsync<PersonData>() retrieves data as a list of FirebaseObject<PersonData>
                // The key is the unique ID generated by Firebase for each entry
                var people = await firebaseDbClient
                    .Child("people") // The path in your Firebase Realtime Database
                    .OnceAsync<PersonData>(); // Retrieve all children under "people"

                // Iterate through the retrieved data and count genders
                foreach (var person in people)
                {
                    total++; // Increment total count for each entry

                    // Check the Gender property (case-insensitive comparison is safer)
                    if (person.Object.Gender != null)
                    {
                        if (person.Object.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                        {
                            male++;
                        }
                        else if (person.Object.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                        {
                            female++;
                        }
                    }
                }

                // Update the labels on the UI thread
                // Since this is an async method called from the constructor (which is on the UI thread),
                // direct updates to UI controls are usually safe. If called from a background thread,
                // you would need to use Invoke or BeginInvoke.
                maleCount.Text = male.ToString();
                femaleCount.Text = female.ToString();
                totalPopulationCount.Text = total.ToString();

                Debug.WriteLine($"Population Counts Loaded: Male={male}, Female={female}, Total={total}"); // Log counts

            }
            catch (InvalidOperationException ex)
            {
                // This exception is thrown by GetFirebaseDatabaseClient if the user is not logged in
                Debug.WriteLine($"Authentication Error loading population counts: {ex.Message}");
                // Optionally clear labels or show a message:
                // maleCount.Text = "N/A";
                // femaleCount.Text = "N/A";
                // totalPopulationCount.Text = "N/A";
            }
            catch (Firebase.Database.FirebaseException fex)
            {
                // Catch specific Firebase Database errors (e.g., permission denied due to rules)
                Debug.WriteLine($"Firebase Database Error loading population counts: {fex.Message}");
                MessageBox.Show($"Error loading population data: {fex.Message}\nCheck your database rules and network connection.", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear labels on error
                maleCount.Text = "Error";
                femaleCount.Text = "Error";
                totalPopulationCount.Text = "Error";
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                Debug.WriteLine($"An unexpected error occurred loading population counts: {ex.Message}");
                MessageBox.Show($"An unexpected error occurred while loading population data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear labels on error
                maleCount.Text = "Error";
                femaleCount.Text = "Error";
                totalPopulationCount.Text = "Error";
            }
        }

        private async Task LoadPopulationCountByDateChart()
        {
            // Clear existing data from the spline chart dataset
            splineDataset.DataPoints.Clear();
            splineChart.Datasets.Clear(); // Clear all datasets from the chart before adding the new one

            int male = 0;
            int female = 0;
            int total = 0;

            try
            {
                // Get the Firebase Database client from the service
                var firebaseDbClient = FirebaseAuthenticationService.Instance.GetFirebaseDatabaseClient();

                // Retrieve all data from the "people" node
                var people = await firebaseDbClient
                    .Child("people") // The path in your Firebase Realtime Database
                    .OnceAsync<PersonData>(); // Retrieve all children under "people"

                // Determine the selected filter
                string filter = populationFilter.SelectedItem?.ToString() ?? "All"; // Default to "All" if nothing is selected

                // Filter the data based on the selected gender
                var filteredPeople = people.Where(p =>
                {
                    // Count total and gender regardless of the filter for the labels
                    total++;
                    if (p.Object?.Gender != null) // Added null check for p.Object
                    {
                        if (p.Object.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase)) male++;
                        else if (p.Object.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase)) female++;
                    }


                    // Apply the filter for the chart data
                    if (filter == "All")
                    {
                        return true; // Include all entries
                    }
                    else if (filter == "Male" && p.Object?.Gender != null) // Added null check for p.Object
                    {
                        return p.Object.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase);
                    }
                    else if (filter == "Female" && p.Object?.Gender != null) // Added null check for p.Object
                    {
                        return p.Object.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase);
                    }
                    return false; // Exclude if gender is null or doesn't match filter
                }).ToList(); // Convert to list


                // Group the filtered data by the date the person was added
                // Convert the Unix timestamp back to DateTime for grouping by date part
                var groupedByDate = filteredPeople
                    .Where(p => p.Object?.AddedTimestamp != 0) // Filter out entries with timestamp 0 (or handle appropriately)
                    .GroupBy(p => DateTimeOffset.FromUnixTimeSeconds(p.Object.AddedTimestamp).Date)
                    .OrderBy(g => g.Key); // Order groups by date


                // Add data points to the spline dataset
                foreach (var group in groupedByDate)
                {
                    // X-axis: Date (formatted as string)
                    string dateLabel = group.Key.ToString("yyyy-MM-dd"); // Format the date as you prefer

                    // Y-axis: Count of people added on that date
                    int count = group.Count();

                    splineDataset.DataPoints.Add(dateLabel, count);
                }

                // Update the population count labels
                maleCount.Text = male.ToString();
                femaleCount.Text = female.ToString();
                totalPopulationCount.Text = total.ToString();

                // Optional: Configure the splineChart axes and title
                splineChart.Title.Text = $"Population Added Per Day ({filter})";
                splineChart.Title.Display = true;

                // Configure X-axis (likely Category axis for date strings)
                splineChart.XAxes.GridLines.Display = false; // Hide vertical grid lines
                splineChart.XAxes.Ticks.Display = true;

                // Configure Y-axis (Linear axis for counts)
                splineChart.YAxes.GridLines.Display = true; // Show horizontal grid lines
                splineChart.YAxes.Ticks.Display = true;


                // Add the dataset to the chart
                if (!splineChart.Datasets.Contains(splineDataset))
                {
                    splineChart.Datasets.Add(splineDataset);
                }


                // Update the chart to display the new data
                splineChart.Update();

                Debug.WriteLine($"Spline Chart Data Loaded for filter: {filter}");

            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Authentication Error loading population chart data: {ex.Message}");
                // Handle authentication errors (e.g., user not logged in)
                MessageBox.Show($"You must be logged in to view population data. Error: {ex.Message}", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear chart or show an error message on the chart area
                splineChart.Datasets.Clear();
                splineChart.Update();
                maleCount.Text = "N/A"; femaleCount.Text = "N/A"; totalPopulationCount.Text = "N/A";
            }
            catch (Firebase.Database.FirebaseException fex)
            {
                Debug.WriteLine($"Firebase Database Error loading population chart data: {fex.Message}");
                // Handle specific Firebase Database errors (e.g., permission denied due to rules)
                MessageBox.Show($"Error loading population chart data: {fex.Message}\nCheck your database rules and network connection.", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear chart or show an error message on the chart area
                splineChart.Datasets.Clear();
                splineChart.Update();
                maleCount.Text = "Error"; femaleCount.Text = "Error"; totalPopulationCount.Text = "Error";
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                Debug.WriteLine($"An unexpected error occurred loading population chart data: {ex.Message}");
                MessageBox.Show($"An unexpected error occurred while loading population chart data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear chart or show an error message on the chart area
                splineChart.Datasets.Clear();
                splineChart.Update();
                maleCount.Text = "Error"; femaleCount.Text = "Error"; totalPopulationCount.Text = "Error";
            }
        }

        private void populationFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When the filter changes, reload the population chart data
            Debug.WriteLine($"populationFilter_SelectedIndexChanged fired. Selected: {populationFilter.SelectedItem}"); // Debugging line
            _ = LoadPopulationCountByDateChart(); // Call asynchronously
        }

        private async Task LoadGenderDistributionChart()
        {
            // Clear existing data from the doughnut chart dataset
            doughnutDataset.DataPoints.Clear();
            doughnutChart.Datasets.Clear(); // Clear all datasets from the chart before adding the new one


            int maleCountValue = 0; // Using different variable names to avoid conflict with labels
            int femaleCountValue = 0;

            try
            {
                // Get the Firebase Database client from the service
                var firebaseDbClient = FirebaseAuthenticationService.Instance.GetFirebaseDatabaseClient();

                // Retrieve all data from the "people" node
                var people = await firebaseDbClient
                    .Child("people") // The path in your Firebase Realtime Database
                    .OnceAsync<PersonData>(); // Retrieve all children under "people"

                // Iterate through the retrieved data and count genders
                foreach (var person in people)
                {
                    if (person.Object?.Gender != null) // Added null check for p.Object
                    {
                        if (person.Object.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                        {
                            maleCountValue++;
                        }
                        else if (person.Object.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                        {
                            femaleCountValue++;
                        }
                    }
                }

                // Add data points to the doughnut dataset
                // Doughnut charts use label-value pairs
                if (maleCountValue > 0)
                {
                    doughnutDataset.DataPoints.Add("Male", maleCountValue);
                }
                if (femaleCountValue > 0)
                {
                    doughnutDataset.DataPoints.Add("Female", femaleCountValue);
                }

                doughnutDataset.Label = "Gender Distribution"; // Label for the legend
                doughnutChart.Title.Text = "Gender Distribution";
                doughnutChart.Title.Display = true; // Show the title

                doughnutChart.Legend.Position = Guna.Charts.WinForms.LegendPosition.Right; // Position the legend
                doughnutChart.Legend.Display = true; // Show the legend

                doughnutChart.XAxes.Display = false; // Hide X-axis
                doughnutChart.YAxes.Display = false; // Hide Y-axis


                // Add the dataset to the chart
                if (!doughnutChart.Datasets.Contains(doughnutDataset))
                {
                    doughnutChart.Datasets.Add(doughnutDataset);
                }

                // Update the chart to display the new data
                doughnutChart.Update();

                Debug.WriteLine($"Doughnut Chart Data Loaded (Gender Distribution)");

            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Authentication Error loading gender distribution chart data: {ex.Message}");
                // Handle authentication errors (e.g., user not logged in)
                // Optionally clear chart or show an error message on the chart area
                doughnutChart.Datasets.Clear();
                doughnutChart.Update();
            }
            catch (Firebase.Database.FirebaseException fex)
            {
                Debug.WriteLine($"Firebase Database Error loading gender distribution chart data: {fex.Message}");
                // Handle specific Firebase Database errors (e.g., permission denied due to rules)
                MessageBox.Show($"Error loading gender distribution data: {fex.Message}\nCheck your database rules and network connection.", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear chart or show an error message on the chart area
                doughnutChart.Datasets.Clear();
                doughnutChart.Update();
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                Debug.WriteLine($"An unexpected error occurred loading gender distribution chart data: {ex.Message}");
                MessageBox.Show($"An unexpected error occurred while loading gender distribution data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear chart or show an error message on the chart area
                doughnutChart.Datasets.Clear();
                doughnutChart.Update();
            }
        }


        private async Task LoadPopulationTableData()
        {
            try
            {
                populationTable.EnableHeadersVisualStyles = false; // Disable default visual styles to apply custom ones
                populationTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(238, 27, 104); // Set the background color
                populationTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                // Get the Firebase Database client from the service
                var firebaseDbClient = FirebaseAuthenticationService.Instance.GetFirebaseDatabaseClient();

                // Retrieve all data from the "people" node
                var people = await firebaseDbClient
                    .Child("people") // The path in your Firebase Realtime Database
                    .OnceAsync<PersonData>(); // Retrieve all children under "people"

                // Create a DataTable to hold the data for the DataGridView
                DataTable dt = new DataTable();
                // Define columns for the DataTable
                dt.Columns.Add("Citizen ID", typeof(string)); // Optional: Display the Firebase unique key
                dt.Columns.Add("First Name", typeof(string));
                dt.Columns.Add("Middle Name", typeof(string));
                dt.Columns.Add("Last Name", typeof(string));
                dt.Columns.Add("Gender", typeof(string));
                dt.Columns.Add("Marital Status", typeof(string));
                dt.Columns.Add("Birth Date", typeof(string)); // Display formatted date
                dt.Columns.Add("Birth Place", typeof(string));
                dt.Columns.Add("Barangay", typeof(string));
                dt.Columns.Add("City", typeof(string));
                dt.Columns.Add("Province", typeof(string));
                dt.Columns.Add("Added Date", typeof(string)); // Display formatted date


                // Populate the DataTable with data from Firebase
                foreach (var person in people)
                {
                    DataRow row = dt.NewRow();
                    row["Citizen ID"] = person.Key; // The unique key from Firebase
                    row["First Name"] = person.Object?.FirstName;
                    row["Middle Name"] = person.Object?.MiddleName;
                    row["Last Name"] = person.Object?.LastName;
                    row["Gender"] = person.Object?.Gender;
                    row["Marital Status"] = person.Object?.MaritalStatus;

                    // Convert timestamp to formatted date string for display
                    if (person.Object?.BirthDateTimestamp != 0)
                    {
                        DateTime birthDate = DateTimeOffset.FromUnixTimeSeconds(person.Object.BirthDateTimestamp).LocalDateTime; // Use LocalDateTime for local time zone
                        row["Birth Date"] = birthDate.ToString("yyyy-MM-dd"); // Format as you prefer
                    }
                    else
                    {
                        row["Birth Date"] = "N/A"; // Handle cases with no timestamp
                    }

                    row["Birth Place"] = person.Object?.BirthPlace;
                    row["Barangay"] = person.Object?.BarangayAddress;
                    row["City"] = person.Object?.CityAddress;
                    row["Province"] = person.Object?.ProvinceAddress;

                    // Convert timestamp to formatted date string for display
                    if (person.Object?.AddedTimestamp != 0)
                    {
                        DateTime addedDate = DateTimeOffset.FromUnixTimeSeconds(person.Object.AddedTimestamp).LocalDateTime; // Use LocalDateTime for local time zone
                        row["Added Date"] = addedDate.ToString("yyyy-MM-dd HH:mm"); // Format as you prefer (including time)
                    }
                    else
                    {
                        row["Added Date"] = "N/A"; // Handle cases with no timestamp
                    }

                    dt.Rows.Add(row);
                }

                // Bind the DataTable to the DataGridView
                populationTable.DataSource = dt;

                // Optional: Auto-size columns to fit content
                populationTable.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                Debug.WriteLine("Population Table Data Loaded.");

            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Authentication Error loading population table data: {ex.Message}");
                // Handle authentication errors (e.g., user not logged in)
                MessageBox.Show($"You must be logged in to view population data. Error: {ex.Message}", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear the DataGridView
                populationTable.DataSource = null;
            }
            catch (Firebase.Database.FirebaseException fex)
            {
                Debug.WriteLine($"Firebase Database Error loading population table data: {fex.Message}");
                // Handle specific Firebase Database errors (e.g., permission denied due to rules)
                MessageBox.Show($"Error loading population table data: {fex.Message}\nCheck your database rules and network connection.", "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear the DataGridView
                populationTable.DataSource = null;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                Debug.WriteLine($"An unexpected error occurred loading population table data: {ex.Message}");
                MessageBox.Show($"An unexpected error occurred while loading population table data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally clear the DataGridView
                populationTable.DataSource = null;
            }
        }
    }
}