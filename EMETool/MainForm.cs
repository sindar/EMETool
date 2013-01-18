using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using EMEDrv;

namespace EMETool
{
    public partial class MainForm : Form
    {
        //Глобальная ссылка на экземпляр EME-сервера
        EMEServer MbeServ;

        //Переменные для работы с драйвером - каналами, устройствами, блоками данных и свойствами
        object ptrChannelHandles = new object();
        object ptrChannelNames = new object();
        object ptrDeviceHandles = new object();
        object ptrDeviceNames = new object();
        object ptrDataBlockHandles = new object();
        object ptrDataBlockNames = new object();
        object ptrPropertyData = new object();

        //GetProperties объект не хавает, почему только массив объектов хотя бы из одного элемента нужен
        object[] ptrProperties = new object[1];

        object ptrQuality = new object();
        object ptrTimeStamp = new object();

        //Массивы каналов, устройств, блоков данных и свойств
        object[] ChannelHandles;
        object[] ChannelNames;
        object[] DeviceHandles;
        object[] DeviceNames;
        object[] DataBlockHandles;
        object[] DataBlockNames;
        object[] Properties;

        //Коллекции для отображения данных, полученных от драйвера
        Hashtable htChannels = new Hashtable();
        Hashtable htDevices = new Hashtable();
        Hashtable htDataBlocks = new Hashtable();

        int NumChannels;
        int NumDevices;
        int NumDataBlocks;
        int NumProperties;

        #region функции работы с драйвером

        //Считывание доступных каналов
        public void GetChannels()
        {
            NumChannels = MbeServ.GetChannels(ref ptrChannelHandles, ref ptrChannelNames);

            ChannelHandles = (object[])ptrChannelHandles;
            ChannelNames = (object[])ptrChannelNames;

            htChannels.Clear();

            int i = 0;
            foreach (Object channel in ChannelNames)
            {
                listBoxChannels.Items.Add(channel);
                htChannels.Add(channel, ChannelHandles[i]);
                i++;
            }
        }

        #endregion

        public MainForm()
        {
            InitializeComponent();
            MbeServ = new EMEServer();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            GetChannels();
        }

       
    }
}
