using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace GFW.Sharp.Core.Ciphering
{

    public class Encrypt
    {
        public static readonly String CHARSET = "UTF-8";

        //public static readonly int BLOCK_MAX_FILE = 64 * 1024 * 1024; // 64MB，被加密数据块的字节最大长度，用于文件

        public static readonly int ENCRYPT_SIZE = 30; // 加密数据长度值加密后的字节长度，固定30个字节，解密后固定14个字节

        public static readonly int IV_SIZE = 16; // IV字节长度，16

        public static readonly int NOISE_MAX = 1024 * 4; // 噪音数据最大长度，4K

        private RandomNumberGenerator _secureRandom = null;

        private IBufferedCipher _cipher = null;

        //private KeyGenerator _keyGenerator = null;

        public Encrypt()
        {

            //super();

            _secureRandom = RNGCryptoServiceProvider.Create();

            try
            {
                //AesManaged.Create().key

                //cipher = Cipher.getInstance("AES/CFB/NoPadding"); // Advanced Encryption Standard - Cipher Feedback Mode - No Padding
                _cipher = CipherUtilities.GetCipher("AES/CFB/NoPadding");

                //_cipher.KeySize = 256;

                
                //_keyGenerator = KeyGenerator.getInstance("AES");



            }
            catch (Exception ex)
            {

                throw ex;

            }

        }

        /**
         * 解密
         * 
         * @param key
         *          SecretKey
         * @param encrypt_bytes
         *          头部包含16字节IV的加密数据
         * 
         * @return
         * 				解密数据
         * 
         */
        public byte[] decrypt(SecretKey key, byte[] encrypt_bytes)
        {

            if (key == null || encrypt_bytes == null || encrypt_bytes.Length < IV_SIZE)
            {

                return null;

            }

            byte[] IV = new byte[IV_SIZE];

            byte[] part2 = new byte[encrypt_bytes.Length - IV_SIZE];

            System.Array.Copy(encrypt_bytes, 0, IV, 0, IV.Length);

            System.Array.Copy(encrypt_bytes, IV.Length, part2, 0, part2.Length);

            return decrypt(key, part2, IV);

        }

        /**
         * 解密
         * 
         * @param key
         *          SecretKey
         * @param cipher_data
         *          加密数据
         * @param IV
         *          IV
         * 
         * @return
         * 				解密数据
         * 
         */
        public byte[] decrypt(SecretKey key, byte[] cipher_data, byte[] IV)
        {

            if (key == null || cipher_data == null || cipher_data.Length == 0 || IV == null || IV.Length == 0)
            {

                return null;

            }

            //IvParameterSpec IVSpec = new IvParameterSpec(IV);
            //ICryptoTransform decryptor = null;
            try
            {

                //_cipher.init(Cipher.DECRYPT_MODE, key, IVSpec);
                _cipher.Init(false, new ParametersWithIV(new KeyParameter(key.Key), IV));


            }
            catch (Exception ex)
            {

                log("初始化Cipher出错：");

                //ex.printStackTrace();

                return null;

            }

            try
            {

                return _cipher.DoFinal(cipher_data);

            }
            catch (Exception ex)
            {

                log("加密数据出错：");

                //ex.printStackTrace();

                return null;

            }

        }

        /**
         * 解密文件
         * 
         * @param key
         *          SecretKey
         * @param src
         *          加密的文件
         * @param dest
         *          解密后的文件
         * @return
         * 				解密是否成功
         */
        #region nouse
        /*
    public bool decryptFile(SecretKey key, File src, File dest)
    {

        if (src == null || !src.exists() || src.isDirectory() || !src.canRead() || dest == null)
        {

            return false;

        }

        long len = src.length(); // 文件大小

        if (len < ENCRYPT_SIZE)
        {

            return false;

        }

        byte[] encrypt_size_bytes = null;

        byte[] encrypt_block_bytes = null;

        InputStream bis = null;

        OutputStream bos = null;

        boolean close = true;

        try
        {

            bis = new BufferedInputStream(new FileInputStream(src));

            bos = new BufferedOutputStream(new FileOutputStream(dest));

            for (;;)
            {

                encrypt_size_bytes = new byte[ENCRYPT_SIZE];

                int read_size = bis.read(encrypt_size_bytes); // 读数据

                if (read_size == -1)
                {

                    break;

                }

                if (read_size != encrypt_size_bytes.length)
                {

                    return false;

                }

                byte[] size_bytes = decrypt(key, encrypt_size_bytes);

                if (size_bytes == null)
                {

                    return false;

                }

                int block_size = getBlockSize(size_bytes);

                if (block_size == 0)
                {

                    return false;

                }

                encrypt_block_bytes = new byte[block_size];

                for (int read_count = 0; read_count < block_size;)
                {

                    read_size = bis.read(encrypt_block_bytes, read_count, block_size - read_count); // 读数据

                    if (read_size == -1)
                    {

                        return false;

                    }

                    read_count += read_size;

                }

                byte[] block_bytes = decrypt(key, encrypt_block_bytes); // 解密数据

                if (block_bytes == null)
                {

                    return false;

                }

                bos.write(block_bytes);

            }

        }
        catch (IOException ex)
        {

            log("解密文件出错：");

            ex.printStackTrace();

            return false;

        }
        finally
        {

            if (bos != null)
            { // 关闭输出流

                try
                {

                    bos.close();

                }
                catch (IOException ex)
                {

                    log("关闭输出流出错：");

                    ex.printStackTrace();

                    close = false;

                }

            }

            if (bis != null)
            { // 关闭输入流

                try
                {

                    bis.close();

                }
                catch (IOException ex)
                {

                    log("关闭输入流出错：");

                    ex.printStackTrace();

                    close = false;

                }

            }

            encrypt_block_bytes = null;

        }

        if (!close || !dest.exists() || dest.length() < len)
        {

            return false;

        }

        return true;

    }
    */
        #endregion

        /**
         * 加密
         * 
         * @param key
         *          SecretKey
         * @param data
         *          数据
         * 
         * @return
         * 				加密数据
         * 
         */
        public byte[] encrypt(SecretKey key, byte[] data)
        {

            if (key == null || data == null)
            {

                return null;

            }

            byte[] IV = getSecureRandom(IV_SIZE);

            //IvParameterSpec IVSpec = new IvParameterSpec(IV);
            
            try
            {

                _cipher.Init(true, new ParametersWithIV(new KeyParameter(key.Key), IV));

            }
            catch (Exception ex)
            {

                log("初始化Cipher出错：");

                //ex.printStackTrace();

                return null;

            }

            byte[] cipher_bytes = null;

            try
            {

                cipher_bytes = _cipher.DoFinal(data);

            }
            catch (Exception ex)
            {

                log("加密数据出错：");

                //ex.printStackTrace();

                return null;

            }

            byte[] iv_cipher_bytes = new byte[cipher_bytes.Length + IV_SIZE];

            System.Array.Copy(IV, 0, iv_cipher_bytes, 0, IV.Length);

            System.Array.Copy(cipher_bytes, 0, iv_cipher_bytes, IV.Length, cipher_bytes.Length);

            return iv_cipher_bytes;

        }

        /**
         * 加密文件
         * 
         * @param key
         *          SecretKey
         * @param src
         *          原文件
         * @param dest
         *          加密文件
         * 
         * @return
         * 				加密是否成功
         * 
         */
        #region nouse

        /*
    public boolean encryptFile(SecretKey key, File src, File dest)
    {

        if (src == null || !src.exists() || src.isDirectory() || !src.canRead() || dest == null)
        {

            return false;

        }

        long len = src.length(); // 文件大小

        if (len == 0L)
        {

            return false;

        }

        byte[] block_bytes = new byte[BLOCK_MAX_FILE];

        byte[] read_bytes = null;

        InputStream bis = null;

        OutputStream bos = null;

        boolean close = true;

        try
        {

            bis = new BufferedInputStream(new FileInputStream(src));

            bos = new BufferedOutputStream(new FileOutputStream(dest));

            for (;;)
            {

                int read_size = bis.read(block_bytes); // 读数据

                if (read_size == -1)
                {

                    break;

                }

                read_bytes = new byte[read_size];

                System.arraycopy(block_bytes, 0, read_bytes, 0, read_size); // 复制实际读到的数据

                byte[] cipher_bytes = encrypt(key, read_bytes); // 加密数据

                if (cipher_bytes == null)
                {

                    return false;

                }

                byte[] size_cipher = encrypt(key, getBlockSizeBytes(cipher_bytes.length)); // 加密数据长度加密，24个字节

                if (size_cipher == null)
                {

                    return false;

                }

                bos.write(size_cipher);

                bos.write(cipher_bytes);

            }

        }
        catch (IOException ex)
        {

            log("加密文件出错：");

            ex.printStackTrace();

            return false;

        }
        finally
        {

            if (bos != null)
            { // 关闭输出流

                try
                {

                    bos.close();

                }
                catch (IOException ex)
                {

                    log("关闭输出流出错：");

                    ex.printStackTrace();

                    close = false;

                }

            }

            if (bis != null)
            { // 关闭输入流

                try
                {

                    bis.close();

                }
                catch (IOException ex)
                {

                    log("关闭输入流出错：");

                    ex.printStackTrace();

                    close = false;

                }

            }

            block_bytes = null;

            read_bytes = null;

        }

        if (!close || !dest.exists() || dest.length() < len)
        {

            return false;

        }

        return true;

    }
    */
        #endregion

        /**
         * 加密网络数据
         * 
         * @param key
         *          SecretKey
         * 
         * @param bytes
         *          原始数据
         * 
         * @return
         * 				[加密数据+噪音数据]长度值的加密数据 + [加密数据 + 噪音数据]
         * 
         */
        public byte[] encryptNet(SecretKey key, byte[] bytes)
        {

            if (key == null || bytes == null || bytes.Length == 0)
            {

                return null;

            }

            byte[] IV = getSecureRandom(IV_SIZE);

            //IvParameterSpec IVSpec = new IvParameterSpec(IV);

            
            try
            {

                //_cipher.init(Cipher.DECRYPT_MODE, key, IVSpec);
                _cipher.Init(true, new ParametersWithIV(new KeyParameter(key.Key), IV));


            }
            catch (Exception ex)
            {

                log("初始化Cipher出错：");

                //ex.printStackTrace();

                return null;

            }

            // 加密数据
            byte[] cipher_bytes = null;

            try
            {

                cipher_bytes = _cipher.DoFinal(bytes);
                

            }
            catch (Exception ex)
            {

                log("加密数据出错：");

                //ex.printStackTrace();

                return null;

            }

            // long start = System.currentTimeMillis();

            // 噪音数据
            byte[] noise_bytes = (cipher_bytes.Length < NOISE_MAX / 2) ? getSecureRandom(new Random().Next(NOISE_MAX)) : new byte[0];

            // long end = System.currentTimeMillis();

            // log("噪音字节长度：" + noise_bytes.length);

            // log("制造噪音时间：" + (end - start));

            // [IV+加密数据+噪音数据]的长度值加密，30个字节
            byte[] size_bytes = encrypt(key, getBlockSizeBytes((IV_SIZE + cipher_bytes.Length), noise_bytes.Length));

            if (size_bytes == null || size_bytes.Length != ENCRYPT_SIZE)
            {

                return null;

            }

            byte[] all_cipher = new byte[size_bytes.Length + IV_SIZE + cipher_bytes.Length + noise_bytes.Length];

            System.Array.Copy(size_bytes, 0, all_cipher, 0, size_bytes.Length);

            System.Array.Copy(IV, 0, all_cipher, size_bytes.Length, IV.Length);

            System.Array.Copy(cipher_bytes, 0, all_cipher, size_bytes.Length + IV.Length, cipher_bytes.Length);

            if (noise_bytes.Length > 0)
            { // 是否加噪音数据

                System.Array.Copy(noise_bytes, 0, all_cipher, size_bytes.Length + IV.Length + cipher_bytes.Length, noise_bytes.Length);

            }

            size_bytes = null;

            IV = null;

            cipher_bytes = null;

            noise_bytes = null;

            return all_cipher;

        }

        /**
         * 还原块长度值
         * 
         * @param bytes
         *          块长度字节数组
         * 
         * @return
         * 				块长度值
         * 
         */
        public int getBlockSize(byte[] bytes)
        {

            if (bytes == null)
            {

                return 0;

            }

            try
            {

                string strSize = Encoding.GetEncoding(CHARSET).GetString(bytes);

                int size = 0;
                int.TryParse(strSize, out size);
                return size;

            }
            catch (Exception ex)
            {

                //ex.printStackTrace();

                return 0;

            }

        }

        /**
         * 生成块长度值的字节数组
         * 
         * @param size
         *          块长度
         * @return
         * 				块长度值字节数组
         */
        public byte[] getBlockSizeBytes(int size)
        {

            try
            {

                return Encoding.GetEncoding(CHARSET).GetBytes(size.ToString());

            }
            catch (Exception ex)
            {

                //ex.printStackTrace();

                return null;

            }

        }

        /**
         * 块长度值转换为字节数组
         * 
         * @param size
         *          加密后的数据块总长度值
         * 
         * @param noise_size
         *          加密前的噪音数据块长度值
         * 
         * @return
         * 				块长度值字节数组
         */
        public byte[] getBlockSizeBytes(int data_size, int noise_size)
        {

            try
            {
                string strBlockSize = string.Format("{0:00000000},{1:00000}", data_size, noise_size);
                return Encoding.GetEncoding(CHARSET).GetBytes(strBlockSize);
            }
            catch (Exception ex)
            {

                //ex.printStackTrace();

                return null;

            }

        }

        /**
         * 从字节数组还原块长度值
         * 
         * @param bytes
         *          长度值字节数组，格式 %08d,%05d
         * @return int[2]
         */
        public int[] getBlockSizes(byte[] bytes)
        {

            if (bytes == null)
            {

                return null;

            }

            try
            {

                string strSizes = Encoding.GetEncoding(CHARSET).GetString(bytes);

                if (strSizes.Split(',').Length != 2)
                {

                    return null;

                }

                String[] sizes = strSizes.Split(',');

                return new int[] { int.Parse(sizes[0]), int.Parse(sizes[1]) };

            }
            catch (Exception ex)
            {

                //ex.printStackTrace();

                return null;

            }

        }

        /**
         * 生成256位SecretKey
         * 
         * @return
         * 				256位SecretKey
         * 
         */
        public SecretKey getKey()
        {

            return getKey(256);

        }

        /**
         * 生成指定加密位数的AES SecretKey
         * 
         * @param bits
         *          加密位数
         * 
         * @return
         * 				SecretKey
         * 
         */
        public SecretKey getKey(int bits)
        {

            if (bits < 128)
            {

                return null;

            }

            try
            {
                AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
                aesProvider.KeySize = bits;
                aesProvider.GenerateKey();

                return new SecretKey { Key=aesProvider.Key, Algorithym="AES" };

            }
            catch (Exception ex)
            {

                log("生成AES SecretKey出错：");

                //ex.printStackTrace();

                return null;

            }

        }

        /**
         * 使用密码生成SecretKey
         * 
         * @param password
         *          密码，必须符合isPassword()要求的标准
         * 
         * @return
         * 				SecretKey
         * 
         */
        public SecretKey getPasswordKey(string password)
        {

            if (!isPassword(password))
            {

                return null;

            }

            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] bs = Encoding.GetEncoding(CHARSET).GetBytes(password);
                bs = x.ComputeHash(bs);
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                foreach (byte b in bs)
                {
                    s.Append(b.ToString("x2").ToLower());
                }


                return getSecretKey(s.ToString());

            }
            catch (Exception ex)
            {

                log("使用密码生成SecretKey出错：");

                //ex.printStackTrace();

                return null;

            }

        }

        /**
         * 使用SecretKey字符串还原SecretKey
         * 
         * @param stringKey
         *          SecretKey字符串
         * 
         * @return
         * 				SecretKey
         * 
         */
        public SecretKey getSecretKey(String stringKey)
        {

            if (stringKey == null || (stringKey = stringKey.Trim()).Length == 0)
            {

                return null;

            }

            byte[] bytes = Convert.FromBase64String(stringKey);

            return new SecretKey { Key = bytes, Algorithym = "AES" };

        }

        /**
         * 生成指定长度的SecureRandom
         * 
         * @param size
         *          指定长度
         * @return
         */
        public byte[] getSecureRandom(int size)
        {

            byte[] bytes = new byte[size];

            _secureRandom.GetBytes(bytes);

            return bytes;

        }

        /**
         * 获取SecretKey的字符串
         * 
         * @param secretKey
         *          SecretKey
         * 
         * @return
         * 				SecretKey的字符串
         * 
         */
        public String getStringKey(SecretKey secretKey)
        {

            if (secretKey == null)
            {

                return null;

            }

            return Convert.ToBase64String(secretKey.Key);

        }

        /**
         * 检查密码是否合格
         * 
         * 1、长度至少为八个字符
         * 2、至少包含一个数字
         * 3、至少包含一个大写字母
         * 4、至少包含一个小写字母
         * 5、不得包含空格
         * 
         * @param password
         * @return
         */
        public static bool isPassword(String password)
        {

            if (password == null || !Regex.IsMatch(password, "^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=\\S+$).{8,}$"))
            {

                return false;

            }

            return true;

            /*
             * 2、至少包含一个数字
             * 3、至少包含一个大写字母
             * 4、至少包含一个小写字母
             * 5、不得包含空格
             */
            // return password.matches("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=])(?=\\S+$).{8,}$");

        }

        /**
         * 打印信息
         * 
         * @param o
         */
        private void log(Object o)
        {

            String time = DateTime.Now.ToString();

            System.Console.WriteLine("[" + time + "] " + o.ToString());

        }

        public void test()
        {

            byte[] bytes = getBlockSizeBytes(888, 0);

            byte[] size_bytes = encrypt(getKey(), bytes);

            log("长度：" + size_bytes.Length);

            //log(new String(bytes));

            int[] sizes = this.getBlockSizes(bytes);

            System.Console.WriteLine(sizes[0]);

            System.Console.WriteLine(sizes[1]);

        }

        #region nouse
        /*
        public void testEncryptFile() 
        {

            SecretKey key = getPasswordKey("abc123");

            log("文件加密测试");

            String ext = ".msi";

            File f1 = new File("d:/" + 1 + ext);

            File f2 = new File("d:/" + 2 + ext);

            File f3 = new File("d:/" + 3 + ext);

        long start = System.currentTimeMillis();

        encryptFile(key, f1, f2);

        long end = System.currentTimeMillis();

        log("时间：" + (end - start));

        log("每秒：" + f1.length() * 1000 / (end - start) / 1024 / 1024 + "M");

        log("文件解密测试");

        start = System.currentTimeMillis();

        decryptFile(key, f2, f3);

        end = System.currentTimeMillis();

        log("时间：" + (end - start));

        log("每秒：" + f2.length() * 1000 / (end - start) / 1024 / 1024 + "M");

	}
    */
        #endregion

        public void testIsPassword()
        {

            String password = "xxXxab12";

            log(isPassword(password));

        }

        public void testSecureRandom()
        {

            // secureRandom.nextBytes(bytes);

            // secureRandom.nextBytes(bytes);

            long start = DateTime.Now.Millisecond;

            byte[] bytes = this.getSecureRandom(1024);

            long end = DateTime.Now.Millisecond;

            log("时间：" + (end - start));

            try
            {

                log(Encoding.GetEncoding(CHARSET).GetString(bytes));

            }
            catch (Exception ex)
            {

                //ex.printStackTrace();

            }

        }

        public byte[] simpleEncrypt(byte[] data)
        {
            byte[] ret = new byte[data.Length];
            System.Array.Copy(data, 0, ret, 0, data.Length);
            for(int i=0;i<ret.Length;i++)
            {
                if(ret[i]==255)
                {
                    ret[i] = 0;
                }
                else
                {
                    ret[i] += 1;
                }
            }
            return ret;
        }

        public byte[] simpleDecrypt(byte[] cipher)
        {
            byte[] ret = new byte[cipher.Length];
            System.Array.Copy(cipher, 0, ret, 0, cipher.Length);
            for (int i = 0; i < ret.Length; i++)
            {
                if (ret[i] == 0)
                {
                    ret[i] = 255;
                }
                else
                {
                    ret[i] -= 1;
                }
            }
            return ret;
        }

    }
}
