using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using EMEDrv;

namespace EMETool
{
    public partial class MainForm : Form
    {
        Server OPCServ;

        public MainForm()
        {
            InitializeComponent();
            OPCServ = new Server();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {   
            CheckServer();
            OPCServ.GetChannels();
            RefreshChannelsListBox();

            for (int i = 0; i < 10; i++)
            {
                DataBlocksGridView.Rows.Add();
                DataBlocksGridView.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        private void listBoxChannels_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBoxChannels.SelectedItem != null)
            {
                OPCServ.GetDevices(listBoxChannels.SelectedItem.ToString());
                RefreshDevicesListBox();
            }
        }

        private void buttonStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!OPCServ.CheckServer())
                {
                    OPCServ.Start();
                }
                else
                {
                    OPCServ.Stop();
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
                RefreshDataBlocksGridView();
        }

        private void listBoxDevices_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBoxDevices.SelectedItem != null)
            {
                OPCServ.GetDataBlocks(listBoxDevices.SelectedItem.ToString());
                RefreshDataBlokcsListBox();
            }
        }

        private void listBoxDataBlocks_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBoxDataBlocks.SelectedItem != null)
            {
                RefreshDataBlocksGridView();
                Refreshtimer.Enabled = true;
            }

            //MbeServ.GetPropertyData(Convert.ToInt32(htDataBlocks[listBoxDataBlocks.SelectedItem.ToString()]), "Name", ref ptrPropertyData);
            //label2.Text = ptrPropertyData.ToString();*/
        }

        private void DataBlocksGridView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                label1.Text = Convert.ToString(DataBlocksGridView.CurrentCellAddress.X + DataBlocksGridView.CurrentCellAddress.Y * 10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DataBlocksGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            OPCServ.WriteData(listBoxDataBlocks.SelectedItem.ToString(), DataBlocksGridView.CurrentCellAddress.X + DataBlocksGridView.CurrentCellAddress.Y * 10, DataBlocksGridView.CurrentCell.Value);
        }

        //Проверка запущен ли драйвер
        public bool CheckServer()
        {
            if (OPCServ.CheckServer())
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
        
        //Построение списка каналов
        public void RefreshChannelsListBox()
        {
            listBoxChannels.Items.Clear();

            foreach (Object channel in OPCServ.ChannelNames)
            {
                listBoxChannels.Items.Add(channel);
            }
        }

        //Построение списка устройств
        public void RefreshDevicesListBox()
        {
            listBoxDevices.Items.Clear();
            
            foreach (Object device in OPCServ.DeviceNames)
            {
                listBoxDevices.Items.Add(device);
            }
        }

        //Построение списка блоков данных
        public void RefreshDataBlokcsListBox()
        {
            listBoxDataBlocks.Items.Clear();
            
            foreach (Object datablock in OPCServ.DataBlockNames)
            {
                listBoxDataBlocks.Items.Add(datablock);
            }
        }

        //Обновление таблицы данных
        public void RefreshDataBlocksGridView()
        {
            object[] Data;

            try
            {
                Data = OPCServ.ReadData(listBoxDataBlocks.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            for (int counter = 0; counter <= Data.Length - 1; counter++)
            {
                DataBlocksGridView[counter % 10, counter / 10].Value = Data[counter];
                
                //--------------------------------Пляска с соответствиями типов интегеров---------------------------
                /*if (Convert.ToInt32(Data[counter]) < 0)
                {
                    DataBlocksGridView[counter % 10, counter / 10].Value = Convert.ToInt32(Data[counter]);// +65536;
                }
                else
                {
                    DataBlocksGridView[counter % 10, counter / 10].Value = Data[counter];
                }*/

            }

        }

    }
}
