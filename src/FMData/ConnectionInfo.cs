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

        #region FileMaker Cloud

        /*
         * Using Claris ID for authentication
         * -------------------------------------------
         * If you want to use the FileMaker Data API with FileMaker Cloud, you must authenticate using your Claris ID account.
         * FileMaker Cloud uses Amazon Cognito for authentication.
         * 
         * FileMaker Cloud provides the following endpoint to gather the required informations:
         * https://www.ifmcloud.com/endpoint/userpool/2.2.0.my.claris.com.json 
         */

        /// <summary>
        /// AWS Cognito UserPoolID
        /// <see href="https://www.ifmcloud.com/endpoint/userpool/2.2.0.my.claris.com.json">data.UserPool_ID</see>
        /// </summary>
        public string CognitoUserPoolID { get; set; } = "us-west-2_NqkuZcXQY";
        /// <summary>
        /// AWS Cognito ClientID
        /// <see href="https://www.ifmcloud.com/endpoint/userpool/2.2.0.my.claris.com.json">data.Client_ID</see>
        /// </summary>
        public string CognitoClientID { get; set; } = "4l9rvl4mv5es1eep1qe97cautn";
        /// <summary>
        /// AWS Cognito Region
        /// <see href="https://www.ifmcloud.com/endpoint/userpool/2.2.0.my.claris.com.json">data.Region</see>
        /// </summary>
        public string RegionEndpoint { get; set; } = "us-west-2";

        #endregion
    }
}
