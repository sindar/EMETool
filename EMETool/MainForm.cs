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

        //Считывание доступных устройств на выбранном канале
        public void GetDevices()
        {
            try
            {
                NumDevices = MbeServ.GetDevices(Convert.ToInt32(htChannels[listBoxChannels.SelectedItem.ToString()].ToString()), ref ptrDeviceHandles, ref ptrDeviceNames);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            DeviceHandles = (object[])ptrDeviceHandles;
            DeviceNames = (object[])ptrDeviceNames;

            listBoxDevices.Items.Clear();
            htDevices.Clear();

            int i = 0;
            foreach (Object device in DeviceNames)
            {
                listBoxDevices.Items.Add(device);
                htDevices.Add(device, DeviceHandles[i]);
                i++;
            }
        }

        //Проверка запущен ли драйвер
        public bool CheckServer()
        {
            if (MbeServ.Running)
            {
                buttonStartStop.Text = "Стоп";
                return true;
            }
            else
            {
                buttonStartStop.Text = "Старт";
                return false;
            }
        }

        //Считывание доступных блоков данных на выбранном устройстве
        public void GetDataBlocks()
        {


            try
            {
                NumDataBlocks = MbeServ.GetDataBlocks(Convert.ToInt32(htDevices[listBoxDevices.SelectedItem.ToString()]), ref ptrDataBlockHandles, ref ptrDataBlockNames);
                DataBlockHandles = (object[])ptrDataBlockHandles;
                DataBlockNames = (object[])ptrDataBlockNames;

                listBoxDataBlocks.Items.Clear();
                htDataBlocks.Clear();

                int i = 0;
                foreach (Object datablock in DataBlockNames)
                {
                    listBoxDataBlocks.Items.Add(datablock);
                    htDataBlocks.Add(datablock, DataBlockHandles[i]);
                    i++;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        //Считывание значений из блоков данных
        public void ReadData()
        {
            object[] Data;

            try
            {
                Data = (object[])MbeServ.ReadData(Convert.ToInt32(htDataBlocks[listBoxDataBlocks.SelectedItem.ToString()]), 2, 0, 0, 100, 0, 65535, 0, out ptrTimeStamp, out ptrQuality);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }


            for (int counter = 0; counter <= Data.Length - 1; counter++)
            {
                if (Convert.ToInt32(Data[counter]) < 0)
                {
                    DataBlocksGridView[counter % 10, counter / 10].Value = Convert.ToInt32(Data[counter]) + 65536;
                }
                else
                {
                    DataBlocksGridView[counter % 10, counter / 10].Value = Data[counter];
                }

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
            CheckServer();
            GetChannels();

            for (int i = 0; i < 10; i++)
            {
                DataBlocksGridView.Rows.Add();
                DataBlocksGridView.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        private void listBoxChannels_SelectedValueChanged(object sender, EventArgs e)
        {
            GetDevices();
        }


        private void buttonStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!MbeServ.Running)
                {
                    MbeServ.Start();
                }
                else
                {
                    MbeServ.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Refreshtimer_Tick(object sender, EventArgs e)
        {
            if (CheckServer())
                ReadData();
        }

        private void listBoxDevices_SelectedValueChanged(object sender, EventArgs e)
        {
            GetDataBlocks();
        }

        private void listBoxDataBlocks_SelectedValueChanged(object sender, EventArgs e)
        {
            ReadData();
        }

        private void DataBlocksGridView_SelectionChanged(object sender, EventArgs e)
        {
            label1.Text = Convert.ToString(DataBlocksGridView.CurrentCellAddress.X + DataBlocksGridView.CurrentCellAddress.Y * 10);
        }

       
    }
}
