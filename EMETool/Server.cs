using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using EMEDrv;
using System.Windows.Forms;

namespace EMETool
{
    
    enum ServerObjectType { Driver = 1, Channel, Device, DataBlock}

    class Server
    {
        //Ссылка на экземпляр EME-сервера
        EMEServer EMEServ;

        //Переменные для работы с драйвером - каналами, устройствами, блоками данных и свойствами
        object ptrChannelHandles = new object();
        object ptrChannelNames = new object();
        object ptrDeviceHandles = new object();
        object ptrDeviceNames = new object();
        object ptrDataBlockHandles = new object();
        object ptrDataBlockNames = new object();
        object ptrPropertyData = new object();

        //GetProperties объект не хавает, почему-то только массив объектов хотя бы из одного элемента нужен
        object[] ptrProperties = new object[1];

        object ptrQuality = new object();
        object ptrTimeStamp = new object();

        //Массивы каналов, устройств, блоков данных и свойств
        object[] ChannelHandles;
        public object[] ChannelNames { get; private set; }
        object[] DeviceHandles;
        public object[] DeviceNames { get; private set; }
        object[] DataBlockHandles;
        public object[] DataBlockNames { get; private set; }
        object[] Properties;

        //Коллекции для отображения данных, полученных от драйвера
        public Hashtable htChannels { get; private set; }
        public Hashtable htDevices{ get; private set; }
        public Hashtable htDataBlocks{ get; private set; }

        public int NumChannels { get; private set; }
        public int NumDevices {  get; private set; }
        public int NumDataBlocks {  get; private set; }
        public int NumProperties {  get; private set; }
        public Byte DataBlockLength { get; private set; }
        
        public Server()
        {
            try
            {
                EMEServ = new EMEServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            htChannels = new Hashtable();
            htDevices = new Hashtable();
            htDataBlocks = new Hashtable();
        }

        public void Start()
        {
            EMEServ.Start();
        }

        public void Stop()
        {
            EMEServ.Stop();
        }

        //Проверка запущен ли сервер
        public bool CheckServer()
        {
            try
            {
                return EMEServ.Running;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка драйвера!" + ex.Message);
                return false;
            }
        }

        //Считывание доступных каналов
        public int GetChannels()
        {
            NumChannels = EMEServ.GetChannels(ref ptrChannelHandles, ref ptrChannelNames);

            ChannelHandles = (object[])ptrChannelHandles;
            ChannelNames = (object[])ptrChannelNames;

            htChannels.Clear();

            if (NumChannels > 0)
            {
                int i = 0;
                foreach (Object channel in ChannelNames)
                {
                    htChannels.Add(channel, ChannelHandles[i]);
                    i++;
                }
            }

            return NumChannels;
        }

        //Считывание доступных устройств на выбранном канале
        public int GetDevices(string sChannel)
        {
            try
            {
                NumDevices = EMEServ.GetDevices(Convert.ToInt32(htChannels[sChannel].ToString()), ref ptrDeviceHandles, ref ptrDeviceNames);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }

            DeviceHandles = (object[])ptrDeviceHandles;
            DeviceNames = (object[])ptrDeviceNames;

            htDevices.Clear();

            if (NumDevices > 0)
            {
                int i = 0;
                foreach (Object device in DeviceNames)
                {
                    htDevices.Add(device, DeviceHandles[i]);
                    i++;
                }
            }
            return NumChannels;
        }

        //Считывание доступных блоков данных на выбранном устройстве
        public int GetDataBlocks(string sDevice)
        {
            try
            {
                NumDataBlocks = EMEServ.GetDataBlocks(Convert.ToInt32(htDevices[sDevice]), ref ptrDataBlockHandles, ref ptrDataBlockNames);
                DataBlockHandles = (object[])ptrDataBlockHandles;
                DataBlockNames = (object[])ptrDataBlockNames;

                htDataBlocks.Clear();

                if (NumDataBlocks > 0)
                {
                    int i = 0;
                    foreach (Object datablock in DataBlockNames)
                    {
                        htDataBlocks.Add(datablock, DataBlockHandles[i]);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
            return NumDataBlocks;
        }

        //Считывание значений из блоков данных
        public object[] ReadData(string sDataBlock)
        {
            object[] Data;

            EMEServ.GetPropertyData(Convert.ToInt32(htDataBlocks[sDataBlock]), "Length", ref ptrPropertyData);
            DataBlockLength = Convert.ToByte(ptrPropertyData);

            try
            {
                Data = (object[])EMEServ.ReadData(Convert.ToInt32(htDataBlocks[sDataBlock]), 2, 0, 0, DataBlockLength, 0, 65535, 0, out ptrTimeStamp, out ptrQuality);
                return Data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        //Запись значений в блок данных
        public void WriteData(string sDataBlock, int lItemOffset, object Value)
        {
            try
            {
                EMEServ.WriteData(Convert.ToInt32(htDataBlocks[sDataBlock]), lItemOffset, 0, 0, 65535, 0, Convert.ToInt16(Value));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        public void GetProperties(ServerObjectType objtype)
        {
            NumProperties = EMEServ.GetProperties((Int16)objtype, ref ptrProperties[0]);
            Properties = (object[])ptrProperties[0];
        }
    }
}
