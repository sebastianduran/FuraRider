using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using Android.Gms.Tasks;
using Java.Lang;
using FuraRider.EventListeners;
using Java.Util;

namespace FuraRider.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/UberTheme", MainLauncher = false )]
    public class RegisterActivity : AppCompatActivity
    {
        TextInputLayout fullnameTex;
        TextInputLayout phoneText;
        TextInputLayout emailText;
        TextInputLayout passwordText;
        Button registerButton;
        CoordinatorLayout rootView;
        TextView clickToLogin;

        FirebaseAuth mAuth;
        FirebaseDatabase database;

        TaskCompletionListener taskCompletionListener = new TaskCompletionListener();

        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userInfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        string fullname, phone, email, password;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);

            InitializeFirebase();
            mAuth = FirebaseAuth.Instance;
            ConnectControl();


        }

        void ConnectControl()
        {
            fullnameTex = (TextInputLayout)FindViewById(Resource.Id.nameText);
            phoneText = (TextInputLayout)FindViewById(Resource.Id.phoneNumber);
            emailText = (TextInputLayout)FindViewById(Resource.Id.emailText);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordText);
            registerButton = (Button)FindViewById(Resource.Id.registerButton);
            rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
            clickToLogin = (TextView)FindViewById(Resource.Id.clickToLogin);

            registerButton.Click += RegisterButton_Click;
            clickToLogin.Click += ClickToLoginText_Click;

        }

        private void ClickToLoginText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }

        void InitializeFirebase()
                {
                    var app = FirebaseApp.InitializeApp(this);

                    if (app == null)
                    {
                        var options = new FirebaseOptions.Builder()
                            .SetApplicationId("fura-8ceb6")
                            .SetApiKey("AIzaSyDjfL4fliMr75o2NY_WYdh5iOuUuZRYBpU")
                            .SetDatabaseUrl("https://fura-8ceb6.firebaseio.com")
                            .SetStorageBucket("fura-8ceb6.appspot.com")
                            .Build();

                        app = FirebaseApp.InitializeApp(this, options);

                        database = FirebaseDatabase.GetInstance(app);
                    }
                    else
                    {
                        database = FirebaseDatabase.GetInstance(app);
                    }

                }
        private void RegisterButton_Click(object sender, EventArgs e)
        {
            
            fullname = fullnameTex.EditText.Text;
            phone = phoneText.EditText.Text;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if(fullname.Length < 3)
            {
                Snackbar.Make(rootView, "Ingresar un nombre valido", Snackbar.LengthShort).Show();
                return;
            }
            else if (phone.Length < 10)
            {
                Snackbar.Make(rootView, "Ingresar numero valido", Snackbar.LengthShort).Show();
                return;
            }
            else if (!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Ingresar email valido", Snackbar.LengthShort).Show();
                return;
            }
            else if (password.Length < 8)
            {
                Snackbar.Make(rootView, "Ingresar contraseña con más de 8 caracteres", Snackbar.LengthShort).Show();
                return;
            }

            RegisterUser(fullname, phone, email, password);
        }

        void RegisterUser(string name, string phone, string email, string password)
        {
            taskCompletionListener.Success += TaskCompletionListener_Success;
            taskCompletionListener.Failure += TaskCompletionListener_Failure;
            mAuth.CreateUserWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, taskCompletionListener)
                .AddOnFailureListener(this, taskCompletionListener);
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "Registro de usuario falló", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "Registro de usuario exitoso", Snackbar.LengthShort).Show();

            HashMap userMap = new HashMap();

            userMap.Put("email", email);
            userMap.Put("phone", phone);
            userMap.Put("fullname", fullname);

            DatabaseReference userReference = database.GetReference("users/" + mAuth.CurrentUser.Uid);
            userReference.SetValue(userMap);
        }

        void SaveToSharedPreferences()
        {
            
            editor = preferences.Edit();

            editor.PutString("email", email);
            editor.PutString("fullname", fullname);
            editor.PutString("phone", phone);

            editor.Apply();
        }

        void RetriveData()
        {
            string email = preferences.GetString("email", "");
        }
    }
}