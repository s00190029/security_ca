using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SSD_Assignment___Banking_Application
{
    public class KeyContainer
    {
        private static void GenKey_SaveInContainer(string containerName)
        {
            // Create the CspParameters object and set the key container
            // name used to store the RSA key pair.
            var parameters = new CspParameters
            {
                KeyContainerName = containerName
            };

            // Create a new instance of RSACryptoServiceProvider that accesses
            // the key container MyKeyContainerName.
            using var rsa = new RSACryptoServiceProvider(parameters);

            // Display the key information to the console.
            Console.WriteLine($"Key added to container: \n  {rsa.ToXmlString(true)}");
        }

        private static void GetKeyFromContainer(string containerName)
        {
            // Create the CspParameters object and set the key container
            // name used to store the RSA key pair.
            var parameters = new CspParameters
            {
                KeyContainerName = containerName
            };

            // Create a new instance of RSACryptoServiceProvider that accesses
            // the key container MyKeyContainerName.
            using var rsa = new RSACryptoServiceProvider(parameters);

            // Display the key information to the console.
            Console.WriteLine($"Key retrieved from container : \n {rsa.ToXmlString(true)}");
        }

        private static void DeleteKeyFromContainer(string containerName)
        {
            // Create the CspParameters object and set the key container
            // name used to store the RSA key pair.
            var parameters = new CspParameters
            {
                KeyContainerName = containerName
            };

            // Create a new instance of RSACryptoServiceProvider that accesses
            // the key container.
            using var rsa = new RSACryptoServiceProvider(parameters)
            {
                // Delete the key entry in the container.
                PersistKeyInCsp = false
            };

            // Call Clear to release resources and delete the key from the container.
            rsa.Clear();

            Console.WriteLine("Key deleted.");
        }
    }

}


