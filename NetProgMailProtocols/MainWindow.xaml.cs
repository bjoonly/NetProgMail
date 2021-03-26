using EAGetMail;
using EASendMail;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MailPriority = EASendMail.MailPriority;

namespace NetProgMailProtocols
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        MailServer mServer;
        MailClient mClient;
        SmtpServer smtpServer;
        SmtpClient smtpClient;
        SmtpMail message;
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {

            FoldersList.Items.Clear();
            mServer = new MailServer(
                "imap.gmail.com",
                emailTB.Text,
                passwordTB.Text,
                EAGetMail.ServerProtocol.Imap4){ SSLConnection = true,Port = 993
            };
           smtpServer = new SmtpServer("smtp.gmail.com")
            {
                Port = 465,
                ConnectType = SmtpConnectType.ConnectSSLAuto,
                User = emailTB.Text,               
               Password = passwordTB.Text
           };
            mClient = new MailClient("TryIt");
            try
            {
                mClient.Connect(mServer);


                foreach (var f in mClient.GetFolders())
                {
                    foreach (var subF in f.SubFolders)
                    {
                        FoldersList.Items.Add(subF.Name);

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            if (  String.IsNullOrEmpty(fromTB.Text) || String.IsNullOrEmpty(toTB.Text) || String.IsNullOrEmpty(themeTB.Text)|| String.IsNullOrEmpty(textTB.Text )) {
                MessageBox.Show("Enter all fields.");
                return;
            }
            SmtpMail message = new SmtpMail("TryIt") 
            {
                From = fromTB.Text,
                To = toTB.Text,
                Subject = themeTB.Text,
                TextBody = textTB.Text,
                Date=DateTime.Now,
                Priority = MailPriority.High,
            };
           
            foreach (string item in attachList.Items)
            {
                message.AddAttachment(item);
            }
             smtpClient = new SmtpClient();
            smtpClient.Connect(smtpServer);

            try
            {
               
                smtpClient.SendMail(message);

            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            }

        private void replyButton_Click(object sender, RoutedEventArgs e)
        {
            Mail m = (Mail)messagesList.SelectedItem;
            if (m == null)
                return;
            fromTB.Text = emailTB.Text;
            toTB.Text = m.From.Address;
            dateTB.Text = "";
            if (!m.Subject.Contains("Re"))
                themeTB.Text = "Re: " + m.Subject;
            else
                themeTB.Text = m.Subject;
            attachList.Items.Clear();
            textTB.Text = "";
        }

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            messagesList.SelectedIndex = -1;
            fromTB.Text = emailTB.Text;
            toTB.Text = "";
            dateTB.Text = "";
            themeTB.Text = "";
            attachList.Items.Clear();
            textTB.Text = "";
        }

        private void attachButton_Click(object sender, RoutedEventArgs e)
        {
            if (fromTB.Text == emailTB.Text && toTB.Text!=emailTB.Text )
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == true)
                    attachList.Items.Add(ofd.FileName);
            }
        }

        private void FoldersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            messagesList.Items.Clear();
            mClient.SelectFolder(mClient.Imap4Folders[1].SubFolders[FoldersList.SelectedIndex]);
            var messages = mClient.GetMailInfos();
            foreach (var item in messages)
            {
                Mail m = mClient.GetMail(item);
                m.Subject = (m.Subject.Replace("(Trial Version)", ""));
                messagesList.Items.Add(m);

            }
        }

        private void messagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Mail m = (Mail)messagesList.SelectedItem;
            if (m == null)
                return;
            fromTB.Text = m.From.Address.ToString();
            toTB.Text = m.To.First().Address.ToString();
            themeTB.Text = m.Subject;
            textTB.Text = m.TextBody;
            dateTB.Text = m.SentDate.ToString();
            attachList.Items.Clear();
            foreach (var a in m.Attachments)
            {
                attachList.Items.Add(a.Name);
            }
        }


    }
}
