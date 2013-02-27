using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Timers;

namespace EMETool
{
    
    class FileOper
    {
        BinaryWriter dataOut;
        BinaryReader dataIn;
        object[] Data;
        string sFileName;

        public FileOper(string sFileName)
        {
            this.sFileName = sFileName;
        }

        public void ExportData(ref Server OPCServ)
        {
            try
            {
                dataOut = new BinaryWriter(new FileStream(sFileName, FileMode.Create));
            }
            catch (IOException ex)
            {
                MessageBox.Show("Ошибка создания файла!" + ex.Message);
                return;
            }
            
            int i = 0;
            foreach (string sDataBlockName in OPCServ.DataBlockNames)
            {
                dataOut.Write("data" + i);

                Data = OPCServ.ReadData(sDataBlockName);
                dataOut.Write(OPCServ.DataBlockLength);

                foreach (Object value in Data)
                {
                    try
                    {
                        dataOut.Write(Convert.ToUInt16(value));
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("Ошибка записи данных в файл!" + ex.Message);
                        dataOut.Close();
                        return;
                    }
                }
                i++;
            }

            if (dataOut != null)
                dataOut.Close();
        }

        public void ImportData(ref Server OPCServ)
        {
            try
            {
                dataIn = new BinaryReader(new FileStream(sFileName, FileMode.Open));
            }
            catch (IOException ex)
            {
                MessageBox.Show("Ошибка открытия файла!" + ex.Message);
                return;
            }

            foreach (string sDataBlockName in OPCServ.DataBlockNames)
            {
                string sTemp;
                Byte Len;
                try
                {
                    sTemp = dataIn.ReadString();
                    Len = dataIn.ReadByte();
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Ошибка чтения данных из файла! Возможно неправильный формат файла." + ex.Message);
                    dataIn.Close();
                    return;
                }

                for (int i = 0; i < Len; i++)
                {
                       
                    Int16 value;
                    try
                    {
                        value = dataIn.ReadInt16();
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("Ошибка чтения данных из файла!" + ex.Message);
                        dataIn.Close();
                        return;
                    }
                    
                    OPCServ.WriteData(sDataBlockName, i, value);
                    
                    for (int j = 0; j < 10000000; j++) ;
                }
            }

            if (dataIn != null)
                dataIn.Close();
        }
    }
}
