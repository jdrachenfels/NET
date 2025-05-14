using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClsLib
{

    public class ClsAdminUser

    {
        // Local variables
        private string _username = "";
        private string _password = "";
        private bool _authenticated = false;

        // Properties
        public string Username
        {
            get => _username;
            set
            { _username = value.Trim().ToLower(); }
        }
        public string Password
        {
            get => _password;
            set
            { _password = value; }
        }


        // Functions

        public Boolean Auth()
        {
            // Sample Auth
            if (_username == "admin" && _password == "drache123")
            {
                _authenticated = true;
            }
            else
            {
                _authenticated = false;
            }
            return _authenticated;
        }

    }
}
