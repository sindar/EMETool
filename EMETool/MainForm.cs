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
        public static string sGlobalError { get; set; }

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
            for (int i = 0; i < 13; i++)
            {
                DataBlocksGridView.Rows.Add();
                DataBlocksGridView.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }

            // При загрузке формы проверяем запущен ли драйвер. 
            if (OPCServ.CheckServer() == 1)
            {
                //Если запущен инициализируем каналы устройства и блоки данных. Заполняем форму.
                OPCServ.GetChannels();
                RefreshChannelsListBox();
                RefreshDataBlokcsListBox();
                RefreshDataBlokcsListBox();
                RefreshTimer.Enabled =  true;
            }
        }

        //Обработчик события выбора канала
        private void listBoxChannels_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBoxChannels.SelectedItem != null)
            {
                OPCServ.GetDevices(listBoxChannels.SelectedItem.ToString());
                RefreshDevicesListBox();
            }
        }

        //Обработчик события выбора устройства
        private void listBoxDevices_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBoxDevices.SelectedItem != null)
            {
                OPCServ.GetDataBlocks(listBoxDevices.SelectedItem.ToString());
                RefreshDataBlokcsListBox();
            }
        }

        //Обработчик события выбора блока данных
        private void listBoxDataBlocks_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBoxDataBlocks.SelectedItem != null)
            {
                RefreshDataBlocksGridView(listBoxDataBlocks.SelectedItem.ToString());
                RefreshTimer.Enabled = true;
            }
        }

        //Кнопка запуска/останова сервера
        private void buttonStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (OPCServ.CheckServer()==0)
                    OPCServ.Start();
                else if (OPCServ.CheckServer() == 1)
                    OPCServ.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Таймер обновления данных
        private void Refreshtimer_Tick(object sender, EventArgs e)
        {
            if (OPCServ.CheckServer() == 1)
            {
                buttonStartStop.Text = "Стоп";
                RefreshDataBlocksGridView(listBoxDataBlocks.SelectedItem.ToString());
            }
            else if (OPCServ.CheckServer() == 0)
                buttonStartStop.Text = "Старт";
            else if ((OPCServ.CheckServer() == -1))
            {
                RefreshTimer.Enabled = false;
                MessageBox.Show("Ошибка драйвера! " + sGlobalError +  "Программа будет закрыта");
                this.Close();
            }
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

        //Обработчик события ввода данных в ячейку
        private void DataBlocksGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            Int16 iValue;

            try
            {
                if (Convert.ToUInt16(DataBlocksGridView.CurrentCell.Value) > 32767)
                    iValue = (Int16)(-65536 + Convert.ToUInt16(DataBlocksGridView.CurrentCell.Value));
                else
                    iValue = Convert.ToInt16(DataBlocksGridView.CurrentCell.Value);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Недопустимый ввод! " + ex.Message);
                return;
            }

            OPCServ.WriteData(listBoxDataBlocks.SelectedItem.ToString(), DataBlocksGridView.CurrentCellAddress.X + DataBlocksGridView.CurrentCellAddress.Y * 10, iValue);
        }

        //Построение списка каналов
        public void RefreshChannelsListBox()
        {
            listBoxChannels.Items.Clear();

            if (OPCServ.NumChannels > 0)
            {
                foreach (Object channel in OPCServ.ChannelNames)
                {
                    listBoxChannels.Items.Add(channel);
                }
                listBoxChannels.SetSelected(0, true);
            }
        }

        //Построение списка устройств
        public void RefreshDevicesListBox()
        {
            listBoxDevices.Items.Clear();

            if (OPCServ.NumDevices > 0)
            {
                foreach (Object device in OPCServ.DeviceNames)
                {
                    listBoxDevices.Items.Add(device);
                }
                listBoxDevices.SetSelected(0, true);
            }
        }

        //Построение списка блоков данных
        public void RefreshDataBlokcsListBox()
        {
            listBoxDataBlocks.Items.Clear();

            if (OPCServ.NumDataBlocks > 0)
            {
                foreach (Object datablock in OPCServ.DataBlockNames)
                {
                    listBoxDataBlocks.Items.Add(datablock);
                }
                listBoxDataBlocks.SetSelected(0, true);
            }       
        }

        //Обновление таблицы данных
        public void RefreshDataBlocksGridView(string sDataBlock)
        {
            object[] Data;

            //Data = OPCServ.ReadData(listBoxDataBlocks.SelectedItem.ToString());
            Data = OPCServ.ReadData(sDataBlock);

            DataBlocksGridView.SelectAll();
            
            for (int j = 0; j < 10; j++)
                for (int i = 0; i < 13; i++)
                    DataBlocksGridView[j, i].Value = "";

            for (int counter = 0; counter <= Data.Length - 1; counter++)
            {
                DataBlocksGridView[counter % 10, counter / 10].Value = Convert.ToUInt16(Data[counter]);
                
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

        private void ExportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileOper file = new FileOper("test");
            file.ExportData(ref OPCServ);
        }

        private void ImportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileOper file = new FileOper("test");
            file.ImportData(ref OPCServ);
        }
    }
}
