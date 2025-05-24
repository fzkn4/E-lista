# E-lista

**E-lista** is a C# Windows Forms application that serves as a practical example for integrating Firebase Authentication and Realtime Database. Beyond its core functionality of managing lists and user data, this repository aims to comprehensively illustrate the process of connecting C# desktop applications to Firebase's powerful backend services.

---

## ✨ Features

* **User Authentication:** Secure user registration and login using Firebase Authentication (Email/Password).
* **Real-time Data Storage:** Store and retrieve user-specific and shared data using Firebase Realtime Database.
* **CRUD operations for list items**
* **Intuitive user interface**
* **Data synchronization**

## 🚀 Technologies Used

* **C#**
* **.NET 8.0 (Windows Forms)**
* **Firebase Authentication.net:** For user authentication.
* **FirebaseDatabase.net:** For Realtime Database interactions.
* **System.Configuration.ConfigurationManager:** For loading configuration from `firebaseConfig.json`.
* **Guna.UI2.WinForms:** For UI
* **Guna.Charts.WinForms:** For Charts

## ⚙️ Setup and Installation

Follow these steps to get your E-lista project up and running locally.

### Prerequisites

* **Visual Studio 2022** (or newer) with **.NET desktop development** workload installed.
* **.NET 8.0 SDK** (usually included with Visual Studio 2022).
* **Internet connection** for Firebase access.

### 1. Clone the Repository

```bash
git clone [https://github.com/fzkn4/E-lista.git](https://github.com/fzkn4/E-lista.git)
cd E-lista
```
##
### 2. Firebase Project Setup

You need to set up a Firebase project in the Google Firebase Console to provide backend services for authentication and data storage.

* **Go to Firebase Console:** [https://console.firebase.google.com/](https://console.firebase.google.com/)
* Create a new project or select your existing E-lista project.

#### a. Enable Authentication

1.  In your Firebase project, navigate to **Build > Authentication** from the left-hand menu.
2.  Go to the **"Sign-in method"** tab.
3.  Ensure that the **"Email/Password"** provider is **enabled**. If not, click on it and toggle the enable switch.

#### b. Set up Realtime Database

1.  In your Firebase project, navigate to **Build > Realtime Database** from the left-hand menu.
2.  Your database will likely use the following primary paths for data:
    * `/users`: To store user-specific data (e.g., profiles, individual lists).
    * `/people`: To store shared data or a list of people.
3.  Go to the **"Rules"** tab within the Realtime Database section.
4.  Replace the default rules with the following JSON. These rules are designed to allow authenticated users to manage their own data and interact with shared data as needed.

    ```json
    {
      "rules": {
        // Users can read/write their own data based on their UID
        "users": {
          "$uid": { 
            ".read": "auth != null && auth.uid == $uid",
            ".write": "auth != null && auth.uid == $uid"
          }
        },
        // Authenticated users can read and write to the 'people' collection
        "people": { 
          ".read": "auth != null",
          ".write": "auth != null"
        },
        // Default to no public read/write access for other paths
        ".read": false, 
        ".write": false 
      }
    }
    ```
5.  Click **"Publish"** to apply these rules to your database.
##
### 3. Configure Your Application

This project utilizes a `firebaseConfig.json` file to store Firebase credentials and other application settings. This is a common and flexible configuration approach for modern .NET applications. For security reasons, this file is intentionally ignored by Git and should **not** be committed to public repositories.

#### a. Add NuGet Packages

Ensure you have the necessary NuGet packages installed to work with JSON configuration:

1.  In Visual Studio, right-click on your project in **Solution Explorer**.
2.  Select **"Manage NuGet Packages..."**.
3.  Go to the **"Browse"** tab.
4.  Install the following packages:
    * `Microsoft.Extensions.Configuration`
    * `Microsoft.Extensions.Configuration.Json`
    * `Microsoft.Extensions.Configuration.FileExtensions` (often installed as a dependency of `Json`, but good to ensure).

#### b. Create `firebaseConfig.json`

This file will hold your Firebase API keys and URLs.

1.  In **Solution Explorer**, right-click on your project (`C# with firebase`).
2.  Select **Add > New Item...**.
3.  Choose **"JSON File"** from the list of templates.
4.  Name the file `firebaseConfig.json` and click **"Add"**.
5.  Add the following structure to your `firebaseConfig.json` file, replacing the placeholder values with your actual Firebase credentials obtained from the Firebase Console (as explained in step 2c of the Firebase Project Setup):

    ```json
    {
      "FirebaseConfig": {
        "ApiKey": "YOUR_FIREBASE_API_KEY_HERE",
        "DatabaseUrl": "YOUR_FIREBASE_DATABASE_URL_HERE"
      },
      "AuthDomain": "YOUR_FIREBASE_AUTH_DOMAIN_HERE"
    }
    ```
    **Remember to replace `YOUR_FIREBASE_API_KEY_HERE`, `YOUR_FIREBASE_DATABASE_URL_HERE`, and `YOUR_FIREBASE_AUTH_DOMAIN_HERE` with your actual credentials!**

#### c. Set File Properties in Visual Studio

Ensure `firebaseConfig.json` is copied to your application's output directory when built:

1.  In **Solution Explorer**, click on `firebaseConfig.json`.
2.  In the **Properties** window (press F4 if you don't see it), set:
    * **"Copy to Output Directory"** to **"Copy if newer"**.
    * **"Build Action"** to **"Content"**.

#### d. Add to `.gitignore` (Optional if you want to put it on Github)

To prevent your sensitive API keys from being committed to GitHub, add `firebaseConfig.json` to your `.gitignore` file:

1.  Open the `.gitignore` file in the root of your Git repository (where your `.sln` file is).
2.  Add the following lines:

    ```
    # Ignore application settings containing secrets
    firebaseConfig.json
    ```
3.  Save the `.gitignore` file. If you've ever committed `firebaseConfig.json` before, you'll need to untrack it first: `git rm --cached firebaseConfig.json` then commit.

#### e. Initialize Configuration in `Program.cs`

Modify your application's entry point (`Program.cs`) to build and expose the configuration:

```csharp
// In Program.cs

using Microsoft.Extensions.Configuration; // Add this using directive
using System.IO; // Add this using directive for Directory

namespace C__with_firebase
{
    public static class Program
    {
        // Public static property to make configuration accessible globally
        public static IConfiguration Configuration { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Build the configuration from firebaseConfig.json
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Looks for firebaseConfig.json in the application's current directory
                .AddJsonFile("firebaseConfig.json", optional: false, reloadOnChange: true) // Loads firebaseConfig.json
                .Build();

            Application.Run(new LoginForm()); // Or your main form
        }
    }
}
```

##
#### That's it! You're all setup. Good luck on your coding journey! 
