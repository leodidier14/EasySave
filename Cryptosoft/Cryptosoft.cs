using System;
using System.Diagnostics;

namespace Cryptosoft
{
    public class Cryptosoft
    {
        public Cryptosoft()
        {

        }

        public int encrypt(string sourcepath, string targetpath)
        {
            try
            {
                //stopwatch is used to count the encryption time
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Encryptor encryptor = new Encryptor();

                encryptor.encryptDecrypt(sourcepath, targetpath);

                stopwatch.Stop();

                return (int)stopwatch.ElapsedMilliseconds;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

}
