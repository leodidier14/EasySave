using System.IO;
using System.Text;

namespace Cryptosoft
{
    public class Encryptor
    {
        byte[] key;
        byte[] buffer = new byte[4096];

        FileStream fsSource = null;
        FileStream fsTarget = null;
        
        public Encryptor()
        {
            ASCIIEncoding encodedData = new ASCIIEncoding();
            //stock the string key in bytes tab
            key = encodedData.GetBytes("key");
        }

        //crypt the string data using the xor operator 
        public byte[] xor(byte[] data)
        {
            //stock the string data in bytes tab
            byte[] cryptData = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                /*foreach char in string data use xor with one char of the key
                modulo is used to restart the key from character 0 */
                cryptData[i] = (byte)(data[i] ^ key[i % key.Length]);
            }

            return cryptData;
        }

        public void encryptDecrypt(string sourcepath, string targetpath)
        {           
            try { 
                //open reading stream
                using (fsSource = new FileStream(sourcepath, FileMode.Open, FileAccess.Read))
                {
                    //open writting stream
                    using (fsTarget = new FileStream(targetpath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        int bytesRead = 0;

                        //read each byte and call the xor method before write them 
                        while ((bytesRead = fsSource.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fsTarget.Write(xor(buffer), 0, bytesRead);
                        }
                        //clear buffer and write data in the file
                        fsTarget.Flush();
                        buffer = null;
                    }
                }              
            }
            catch (FileNotFoundException)
            {

            }

        }
    }
}
