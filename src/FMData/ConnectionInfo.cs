namespace FMData
{
    /// <summary>
    /// Represents the connection information for FMS.
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// HTTPS based uri to connect to FileMaker Server.
        /// </summary>
        public string FmsUri { get; set; }
        /// <summary>
        /// The name of the database to make connections with.
        /// </summary>
        public string Database { get; set; }
        /// <summary>
        /// Username to use when making the connection.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Password to use when making the connection.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// AWS Cognito UserPoolID
        /// </summary>
        public string CognitoUserPoolID { get; set; } = "us-west-2_NqkuZcXQY";
        /// <summary>
        /// AWS Cognito ClientID
        /// </summary>
        public string CognitoClientID { get; set; } = "4l9rvl4mv5es1eep1qe97cautn";
    }
}
