using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;

namespace LagerLista
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            konektuj();
            dodajInsert();
            dodajUpdate();
            dodajDelete();
            getData();

            datagridP.ItemsSource = ds.Tables[0].DefaultView;
            datagridP.SelectionMode = DataGridSelectionMode.Single;

            datagridK.ItemsSource = ds.Tables[1].DefaultView;
            datagridK.SelectionMode = DataGridSelectionMode.Single;

            datagridPS.ItemsSource = dvP;
            datagridPS.SelectionMode = DataGridSelectionMode.Single;

            datagridKS.ItemsSource = dvK;
            datagridKS.SelectionMode = DataGridSelectionMode.Single;

            odabraniP.ItemsSource = odabrani.DefaultView;
            odabraniP.SelectionMode = DataGridSelectionMode.Single;

            datagridUlazi.ItemsSource = ui.Tables[0].DefaultView;
            datagridUlazi.SelectionMode = DataGridSelectionMode.Single;
        }

        public static SqlCeConnection cn = new SqlCeConnection("Data source='BazePodataka/Baza1.sdf'; LCID=1033");
        public static SqlCeDataAdapter daProizvodi = new SqlCeDataAdapter("SELECT * FROM proizvodi", cn);
        public static SqlCeDataAdapter daAktivni = new SqlCeDataAdapter("SELECT * FROM proizvodi WHERE kolicina != 0", cn);
        public static SqlCeDataAdapter daKupci = new SqlCeDataAdapter("SELECT * FROM kupci", cn);
        public static SqlCeDataAdapter daUlazi = new SqlCeDataAdapter("SELECT * FROM ulazi", cn);
        public static SqlCeDataAdapter daUlaziSpec = new SqlCeDataAdapter("SELECT * FROM ulazi WHERE id = '1'", cn);
        public static DataSet ds = new DataSet();
        public static DataSet aktivniP = new DataSet();
        public static DataSet ui = new DataSet();
        public static DataView dvP;
        public static DataView dvK;
        public static DataTable odabrani = new DataTable();
        public static int flag = 0;
        string adresaFak;
        string racunFak;
        string pibFak;
        decimal osnovica;
        decimal svega;
        decimal rabatUk;
        decimal rabatProc;
        decimal rabatKol;
        bool prRab;
        string putanja;
        string mestoKup;

        public void konektuj()
        {
            putanja = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            try
            {
                cn.Open();
                if (cn.State != System.Data.ConnectionState.Open)
                {
                    MessageBox.Show(
                        "Connection to database failed.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);
                }
            }
            catch (SqlCeException sqlexc)
            {
                MessageBox.Show(
                        sqlexc.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                StreamWriter fssw = new StreamWriter(fs);
                fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlexc.Message + Environment.NewLine + Environment.NewLine + sqlexc.StackTrace + Environment.NewLine);
                fssw.Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                        exc.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                StreamWriter fssw = new StreamWriter(fs);
                fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + exc.Message + Environment.NewLine + Environment.NewLine + exc.StackTrace + Environment.NewLine);
                fssw.Close();
            }
        }

        public void getData()
        {
            daProizvodi.Fill(ds, "proizvodi");
            daKupci.Fill(ds, "kupci");
            daUlazi.Fill(ds, "ulazi");
            daUlaziSpec.Fill(ui, "in");
            ui.Tables[0].Clear();

            if (!Directory.Exists(putanja + @"\Lager lista files"))
            {
                Directory.CreateDirectory(putanja + @"\Lager lista files");
            }

            try
            {
                if (aktivniP.Tables.Count > 0)
                {
                    aktivniP.Tables[0].Clear();
                }
                daAktivni.Fill(aktivniP, "Aktivni");
            }
            catch (Exception klon)
            {
                FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                StreamWriter fssw = new StreamWriter(fs);
                fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + klon.Message + Environment.NewLine + Environment.NewLine + klon.StackTrace + Environment.NewLine);
                fssw.Close();
            }

            dvP = new DataView(aktivniP.Tables[0]);
            dvK = new DataView(ds.Tables[1]);

            odabrani.Clear();
            odabrani.Columns.Add("sifra");
            odabrani.Columns.Add("naziv");
            odabrani.Columns.Add("kol");
            odabrani.Columns.Add("jedinica");
            odabrani.Columns.Add("cena");
            odabrani.Columns.Add("cenaBez");
            odabrani.Columns.Add("rabat");
            odabrani.Columns.Add("cenaTotal");
        }

        public void dodajInsert()
        {
            SqlCeCommand insertP = new SqlCeCommand();
            insertP.Connection = cn;
            insertP.CommandType = CommandType.Text;
            insertP.CommandText = "INSERT INTO proizvodi (sifra, ime, kolicina, jedinica, cena) VALUES (@sifra, @ime, @kolicina, @jedinica, @cena)";
            insertP.Parameters.Add(new SqlCeParameter("@sifra", SqlDbType.NVarChar, 20, "sifra"));
            insertP.Parameters.Add(new SqlCeParameter("@ime", SqlDbType.NVarChar, 50, "ime"));
            insertP.Parameters.Add(new SqlCeParameter("@kolicina", SqlDbType.Float, 8, "kolicina"));
            insertP.Parameters.Add(new SqlCeParameter("@jedinica", SqlDbType.NVarChar, 10, "jedinica"));
            insertP.Parameters.Add(new SqlCeParameter("@cena", SqlDbType.Money, 19, "cena"));
            daProizvodi.InsertCommand = insertP;

            SqlCeCommand insertK = new SqlCeCommand();
            insertK.Connection = cn;
            insertK.CommandType = CommandType.Text;
            insertK.CommandText = "INSERT INTO kupci (ime, adresa, mesto, racun, pib) VALUES (@ime, @adresa, @mesto, @racun, @pib)";
            insertK.Parameters.Add(new SqlCeParameter("@ime", SqlDbType.NVarChar, 50, "ime"));
            insertK.Parameters.Add(new SqlCeParameter("@adresa", SqlDbType.NVarChar, 50, "adresa"));
            insertK.Parameters.Add(new SqlCeParameter("@mesto", SqlDbType.NVarChar, 20, "mesto"));
            insertK.Parameters.Add(new SqlCeParameter("@racun", SqlDbType.NVarChar, 30, "racun"));
            insertK.Parameters.Add(new SqlCeParameter("@pib", SqlDbType.NVarChar, 20, "pib"));
            daKupci.InsertCommand = insertK;

            SqlCeCommand insertU = new SqlCeCommand();
            insertU.Connection = cn;
            insertU.CommandType = CommandType.Text;
            insertU.CommandText = "INSERT INTO ulazi (datum, firma, mesto, ulaz, izlaz, cena, rabat, zaliha, sifraPr) VALUES (@datum, @firma, @mesto, @ulaz, @izlaz, @cena, @rabat, @zaliha, @sifraPr)";
            insertU.Parameters.Add(new SqlCeParameter("@datum", SqlDbType.NVarChar, 20, "datum"));
            insertU.Parameters.Add(new SqlCeParameter("@firma", SqlDbType.NVarChar, 50, "firma"));
            insertU.Parameters.Add(new SqlCeParameter("@mesto", SqlDbType.NVarChar, 50, "mesto"));
            insertU.Parameters.Add(new SqlCeParameter("@ulaz", SqlDbType.Float, 8, "ulaz"));
            insertU.Parameters.Add(new SqlCeParameter("@izlaz", SqlDbType.Float, 8, "izlaz"));
            insertU.Parameters.Add(new SqlCeParameter("@cena", SqlDbType.Money, 19, "cena"));
            insertU.Parameters.Add(new SqlCeParameter("@rabat", SqlDbType.NVarChar, 4, "rabat"));
            insertU.Parameters.Add(new SqlCeParameter("@zaliha", SqlDbType.Float, 8, "zaliha"));
            insertU.Parameters.Add(new SqlCeParameter("@sifraPr", SqlDbType.NVarChar, 20, "sifraPr"));
            daUlazi.InsertCommand = insertU;
        }

        public void dodajUpdate()
        {
            SqlCeCommand updateP = new SqlCeCommand();
            updateP.Connection = cn;
            updateP.CommandType = CommandType.Text;
            updateP.CommandText = "UPDATE proizvodi SET sifra=@sifra, ime=@ime, kolicina=@kolicina, jedinica=@jedinica, cena=@cena WHERE id=@id";
            updateP.Parameters.Add(new SqlCeParameter("@sifra", SqlDbType.NVarChar, 20, "sifra"));
            updateP.Parameters.Add(new SqlCeParameter("@ime", SqlDbType.NVarChar, 50, "ime"));
            updateP.Parameters.Add(new SqlCeParameter("@kolicina", SqlDbType.Float, 8, "kolicina"));
            updateP.Parameters.Add(new SqlCeParameter("@jedinica", SqlDbType.NVarChar, 10, "jedinica"));
            updateP.Parameters.Add(new SqlCeParameter("@cena", SqlDbType.Money, 19, "cena"));
            updateP.Parameters.Add(new SqlCeParameter("@id", SqlDbType.Int, 4, "id"));
            daProizvodi.UpdateCommand = updateP;
        }

        public void dodajDelete()
        {
            SqlCeCommand deleteP = new SqlCeCommand();
            deleteP.Connection = cn;
            deleteP.CommandType = CommandType.Text;
            deleteP.CommandText = "DELETE FROM proizvodi WHERE id=@id";
            SqlCeParameter deleteParamP = new SqlCeParameter("@id", SqlDbType.Int, 4, "id");
            deleteParamP.SourceVersion = DataRowVersion.Original;
            deleteP.Parameters.Add(deleteParamP);
            daProizvodi.DeleteCommand = deleteP;

            SqlCeCommand deleteK = new SqlCeCommand();
            deleteK.Connection = cn;
            deleteK.CommandType = CommandType.Text;
            deleteK.CommandText = "DELETE FROM kupci WHERE id=@id";
            SqlCeParameter deleteParamK = new SqlCeParameter("@id", SqlDbType.Int, 4, "id");
            deleteParamK.SourceVersion = DataRowVersion.Original;
            deleteK.Parameters.Add(deleteParamK);
            daKupci.DeleteCommand = deleteK;
        }

        private void dodajP_Click(object sender, RoutedEventArgs e)
        {
            string sifrap = textboxSifra.Text;
            string imep = textboxIme.Text;
            double kolicinap = 0;
            bool x = Double.TryParse(textboxKol.Text, out kolicinap);
            string jedinicap = combobJedMere.Text;
            decimal cena = 0;
            bool d = Decimal.TryParse(textboxCena.Text, out cena);
            decimal cenaNab = 0;
            bool cnn = Decimal.TryParse(cenaNabavke.Text, out cenaNab);
            string poruka = String.Format(" SIFRA: {0} \n NAZIV: {1} \n KOLICINA: {2} \n JED. MERE: {3}", sifrap, imep, textboxKol.Text, jedinicap);

            if (String.IsNullOrWhiteSpace(sifrap) || String.IsNullOrWhiteSpace(imep) || String.IsNullOrWhiteSpace(jedinicap) || String.IsNullOrWhiteSpace(imeDobavljaca.Text) || String.IsNullOrWhiteSpace(mestoDobavljaca.Text))
            {
                MessageBox.Show(
                    "Sva polja moraju biti popunjena!",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else if (d != true)
            {
                MessageBox.Show(
                    "Greska u unetoj ceni!",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else if (x != true)
            {
                MessageBox.Show(
                    "Greska u unetoj kolicini!",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else if (cnn != true)
            {
                MessageBox.Show(
                    "Greska u unetoj ceni nabavke!",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else
            {
                MessageBoxResult key2 = MessageBox.Show(
                    poruka,
                    "Dodati?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question,
                    MessageBoxResult.OK);
                if (key2 == MessageBoxResult.OK)
                {
                    try
                    {
                        ds.Tables[0].Clear();
                        daProizvodi.Fill(ds, "proizvodi");
                    }
                    catch (SqlCeException sqlcexc5)
                    {
                        MessageBox.Show(
                                sqlcexc5.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlcexc5.Message + Environment.NewLine + Environment.NewLine + sqlcexc5.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }
                    catch (Exception exc5)
                    {
                        MessageBox.Show(
                                exc5.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + exc5.Message + Environment.NewLine + Environment.NewLine + exc5.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if ((string)row["sifra"] == sifrap)
                        {
                            try
                            {
                                double rowkol = Double.Parse(row["kolicina"].ToString());
                                rowkol += kolicinap;
                                row["kolicina"] = rowkol;
                                daProizvodi.Update(ds.Tables[0]);
                                flag = 5;

                                // odavde pocinje
                                if (kolicinap > 0)
                                {
                                    DataRow ulaziRow = ds.Tables[2].NewRow();
                                    ulaziRow["datum"] = System.DateTime.Today.ToShortDateString();
                                    ulaziRow["firma"] = imeDobavljaca.Text;
                                    ulaziRow["mesto"] = mestoDobavljaca.Text;
                                    ulaziRow["ulaz"] = kolicinap;
                                    ulaziRow["izlaz"] = DBNull.Value;
                                    ulaziRow["cena"] = cenaNab;
                                    ulaziRow["rabat"] = "";
                                    ulaziRow["zaliha"] = rowkol;
                                    ulaziRow["sifraPr"] = sifrap.Trim();
                                    ds.Tables[2].Rows.Add(ulaziRow);
                                    daUlazi.Update(ds.Tables[2]);

                                    imeDobavljaca.Text = "";
                                    mestoDobavljaca.Text = "";
                                    cenaNabavke.Text = "";
                                }
                                //ovde se zavrsava

                                MessageBox.Show(
                                    "Proizvod uspesno dodat!",
                                    "Dodato",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                                textboxSifra.Text = "";
                                textboxIme.Text = "";
                                textboxKol.Text = "";
                                textboxCena.Text = "";
                            }
                            catch (SqlCeException sqlcexc2)
                            {
                                MessageBox.Show(
                                sqlcexc2.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                                FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                                StreamWriter fssw = new StreamWriter(fs);
                                fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlcexc2.Message + Environment.NewLine + Environment.NewLine + sqlcexc2.StackTrace + Environment.NewLine);
                                fssw.Close();
                            }
                            catch (Exception exc2)
                            {
                                MessageBox.Show(
                                exc2.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                                FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                                StreamWriter fssw = new StreamWriter(fs);
                                fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + exc2.Message + Environment.NewLine + Environment.NewLine + exc2.StackTrace + Environment.NewLine);
                                fssw.Close();
                            }
                        }
                    }

                    if (flag != 5)
                    {
                        try
                        {
                            DataRow newRowP = ds.Tables[0].NewRow();
                            newRowP["sifra"] = sifrap;
                            newRowP["ime"] = imep;
                            newRowP["kolicina"] = kolicinap;
                            newRowP["jedinica"] = jedinicap;
                            newRowP["cena"] = cena;
                            ds.Tables[0].Rows.Add(newRowP);
                            daProizvodi.Update(ds.Tables[0]);

                            // odavde pocinje
                            if (kolicinap > 0)
                            {
                                DataRow ulaziRow = ds.Tables[2].NewRow();
                                ulaziRow["datum"] = System.DateTime.Today.ToShortDateString();
                                ulaziRow["firma"] = imeDobavljaca.Text;
                                ulaziRow["mesto"] = mestoDobavljaca.Text;
                                ulaziRow["ulaz"] = kolicinap;
                                ulaziRow["izlaz"] = DBNull.Value;
                                ulaziRow["cena"] = cenaNab;
                                ulaziRow["rabat"] = "";
                                ulaziRow["zaliha"] = kolicinap;
                                ulaziRow["sifraPr"] = sifrap.Trim();
                                ds.Tables[2].Rows.Add(ulaziRow);
                                daUlazi.Update(ds.Tables[2]);

                                imeDobavljaca.Text = "";
                                mestoDobavljaca.Text = "";
                                cenaNabavke.Text = "";
                            }
                            //ovde se zavrsava

                            MessageBox.Show(
                                "Proizvod uspesno dodat!",
                                "Dodato",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information,
                                MessageBoxResult.OK);
                        }
                        catch (SqlCeException sqlcexc)
                        {
                            MessageBox.Show(
                                sqlcexc.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlcexc.Message + Environment.NewLine + Environment.NewLine + sqlcexc.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(
                                exc.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + exc.Message + Environment.NewLine + Environment.NewLine + exc.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }
                        textboxSifra.Text = "";
                        textboxIme.Text = "";
                        textboxKol.Text = "";
                        textboxCena.Text = "";
                    }
                    flag = 0;
                    ds.Tables[0].Clear();
                    daProizvodi.Fill(ds, "proizvodi");

                    if (aktivniP.Tables.Count > 0)
                    {
                        aktivniP.Tables[0].Clear();
                    }
                    daAktivni.Fill(aktivniP, "Aktivni");
                }
            }
        }

        private void dodajK_Click(object sender, RoutedEventArgs e)
        {
            string imek = textboxImeK.Text;
            string adresa = textboxAdr.Text;
            string mesto = textboxMesto.Text;
            string racun = textboxRacun.Text;
            string pib = textboxPib.Text;
            string poruka2 = String.Format(" NAZIV: {0} \n ADRESA: {1} \n RACUN: {2} \n PIB: {3} \n", imek, adresa, racun, pib);

            MessageBoxResult key3 = MessageBox.Show(
                poruka2,
                "Dodati?",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question,
                MessageBoxResult.OK);
            if (key3 == MessageBoxResult.OK)
            {
                try
                {
                    DataRow newRowK = ds.Tables[1].NewRow();
                    newRowK["ime"] = imek;
                    newRowK["adresa"] = adresa;
                    newRowK["mesto"] = mesto;
                    newRowK["racun"] = racun;
                    newRowK["pib"] = pib;
                    ds.Tables[1].Rows.Add(newRowK);
                    daKupci.Update(ds.Tables[1]);

                    MessageBox.Show(
                        "Kupac uspesno dodat!",
                        "Dodato",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK);
                }
                catch (SqlCeException sqlcexc3)
                {
                    MessageBox.Show(
                        sqlcexc3.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                    StreamWriter fssw = new StreamWriter(fs);
                    fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlcexc3.Message + Environment.NewLine + Environment.NewLine + sqlcexc3.StackTrace + Environment.NewLine);
                    fssw.Close();
                }
                catch (Exception exc3)
                {
                    MessageBox.Show(
                        exc3.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);

                    FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                    StreamWriter fssw = new StreamWriter(fs);
                    fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + exc3.Message + Environment.NewLine + Environment.NewLine + exc3.StackTrace + Environment.NewLine);
                    fssw.Close();
                }
                ds.Tables[1].Clear();
                daKupci.Fill(ds, "kupci");
                textboxImeK.Text = "";
                textboxAdr.Text = "";
                textboxMesto.Text = "";
                textboxRacun.Text = "";
                textboxPib.Text = "";
            }
        }

        private void obrisiK_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selKupac = datagridK.SelectedItem as DataRowView;
            if (selKupac != null)
            {
                string infoKupac = String.Format(" NAZIV: {0} \n ADRESA: {1} \n RACUN: {2} \n PIB: {3} \n", selKupac.Row.ItemArray[1], selKupac.Row.ItemArray[2], selKupac.Row.ItemArray[3], selKupac.Row.ItemArray[4]);
                MessageBoxResult key4 = MessageBox.Show(
                    infoKupac,
                    "Obrisati?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question,
                    MessageBoxResult.OK);
                if (key4 == MessageBoxResult.OK)
                {
                    try
                    {
                        int kupacID = (int)selKupac.Row.ItemArray[0];
                     
                        ds.Tables[1].Clear();
                        daKupci.Fill(ds, "kupci");
                                                
                        foreach (DataRow row2 in ds.Tables[1].Rows)
                        {
                            if ((int)row2["id"] == kupacID)
                            {
                                row2.Delete();
                                daKupci.Update(ds.Tables[1]);
                                MessageBox.Show(
                                    "Kupac uspesno obrisan!",
                                    "Obrisano",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                                break;
                            }
                        }

                    }
                    catch (SqlCeException sqlcexc4)
                    {
                        MessageBox.Show(
                            sqlcexc4.Message,
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlcexc4.Message + Environment.NewLine + Environment.NewLine + sqlcexc4.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }
                    catch (Exception exc4)
                    {
                        MessageBox.Show(
                            exc4.Message,
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + exc4.Message + Environment.NewLine + Environment.NewLine + exc4.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show(
                    "Niste odabrali nijednog kupca",
                    "Odaberi!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK);
            }
        }

        private void datagridPS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datagridPS.SelectedItem != null)
            {
                DataRowView redP = (DataRowView)datagridPS.SelectedItem;

                chosenPS.IsEnabled = true;
                chosenPN.IsEnabled = true;
                chosenPK.IsEnabled = true;
                chosenCena.IsEnabled = true;
                chosenRabat.IsEnabled = true;

                if (redP.Row.ItemArray[1] != null)
                {
                    chosenPS.Text = redP.Row.ItemArray[1].ToString();
                }
                else
                {
                    chosenPS.Text = "***GRESKA***";
                }

                if (redP.Row.ItemArray[2] != null)
                {
                    chosenPN.Text = redP.Row.ItemArray[2].ToString();
                }
                else
                {
                    chosenPN.Text = "***GRESKA***";
                }

                if (redP.Row.ItemArray[4] != null)
                {
                    chosenJed.Content = redP.Row.ItemArray[4].ToString();
                }
                else
                {
                    chosenJed.Content = "";
                }

                if (redP.Row.ItemArray[5] != null)
                {
                    chosenCena.Text = redP.Row.ItemArray[5].ToString();
                }
                else
                {
                    chosenCena.Text = "***GRESKA***";
                }
            }

            chosenPK.Text = "";
        }

        private void datagridKS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datagridKS.SelectedItem != null)
            {
                DataRowView redK = (DataRowView)datagridKS.SelectedItem;

                chosenK.IsEnabled = true;

                if (redK.Row.ItemArray[1] != null)
                {
                    chosenK.Text = redK.Row.ItemArray[1].ToString();
                }
                else
                {
                    chosenK.Text = "***GRESKA***";
                }
            }
        }
                
        private void chose_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(chosenPS.Text) || string.IsNullOrWhiteSpace(chosenPN.Text) || string.IsNullOrWhiteSpace(chosenPK.Text) || string.IsNullOrWhiteSpace(chosenCena.Text))
            {
                MessageBox.Show(
                    "Sva polja moraju biti popunjena",
                    "Prazna polja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK);
            }
            else
            {
                decimal chCena = 0;
                decimal chKol = 0;
                decimal chRab;
                bool y = Decimal.TryParse(chosenCena.Text, out chCena);
                bool z = Decimal.TryParse(chosenPK.Text, out chKol);
                bool q = Decimal.TryParse(chosenRabat.Text, out chRab);
                if (y == false)
                {
                    MessageBox.Show(
                        "Greska u unetoj ceni",
                        "Greska",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK
                        );
                }
                else if (z == false)
                {
                    MessageBox.Show(
                       "Greska u unetoj kolicini",
                       "Greska",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error,
                       MessageBoxResult.OK
                       );
                }
                else if (q == false)
                {
                    MessageBox.Show(
                       "Greska u unetom rabatu",
                       "Greska",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error,
                       MessageBoxResult.OK
                       );
                }
                else
                {
                    try
                    {
                        DataRow red = odabrani.NewRow();
                        red["sifra"] = chosenPS.Text;
                        red["naziv"] = chosenPN.Text;
                        red["kol"] = chosenPK.Text;
                        red["jedinica"] = chosenJed.Content.ToString();
                        red["cena"] = chCena.ToString("F");
                        red["cenaBez"] = chCena * chKol;
                        red["rabat"] = chosenRabat.Text + "%";
                        decimal cenaTotW = (chCena * chKol) - (chCena * chKol) / 100 * chRab;
                        red["cenaTotal"] = cenaTotW.ToString("F");
                        odabrani.Rows.Add(red);
                        chosenPS.Text = "";
                        chosenPN.Text = "";
                        chosenPK.Text = "";
                        chosenJed.Content = "";
                        chosenCena.Text = "";
                        chosenRabat.Text = "0";

                        chosenPS.IsEnabled = false;
                        chosenPN.IsEnabled = false;
                        chosenPK.IsEnabled = false;
                        chosenCena.IsEnabled = false;
                        chosenRabat.IsEnabled = false;
                    }
                    catch (SqlCeException sqlOdab)
                    {
                        MessageBox.Show(
                                sqlOdab.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlOdab.Message + Environment.NewLine + Environment.NewLine + sqlOdab.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }
                    catch (Exception excOdab)
                    {
                        MessageBox.Show(
                                    excOdab.Message,
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error,
                                    MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + excOdab.Message + Environment.NewLine + Environment.NewLine + excOdab.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }
                }
            }
        }

        private void searchP_TextChanged(object sender, TextChangedEventArgs e)
        {
            dvP.RowFilter = String.Format("sifra LIKE '%{0}%' OR ime LIKE '%{0}%'", searchP.Text);
        }

        private void searchK_TextChanged(object sender, TextChangedEventArgs e)
        {
                dvK.RowFilter = String.Format("ime LIKE '%{0}%' OR adresa LIKE '%{0}%'", searchK.Text);
        }

        private void choseKup_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(chosenK.Text))
            {
                MessageBox.Show(
                    "Nijedan kupac nije odabran",
                    "Odaberite kupca",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK);
            }
            else
            {
                labelOdK.Content = chosenK.Text;
                chosenK.IsEnabled = false;
            }
        }

        private void ponistiOdabrano_Click(object sender, RoutedEventArgs e)
        {
            odabrani.Clear();
        }

        private void cancelKu_Click(object sender, RoutedEventArgs e)
        {
            labelOdK.Content = "";
        }

        private void fakturisi_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult keyFak = MessageBox.Show(
                "Napraviti izlaz?",
                "Fakturisati?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);
            if (keyFak == MessageBoxResult.Yes)
            {
                decimal PDVx;
                bool x = Decimal.TryParse(chosenPDV.Text, out PDVx);
                if (x == false)
                {
                    MessageBox.Show(
                        "Greska u unetom PDV-u.",
                        "Greska",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning,
                        MessageBoxResult.OK);
                }
                else if (String.IsNullOrWhiteSpace(labelOdK.Content.ToString()))
                {
                    MessageBox.Show(
                        "Morate odabrati kupca.",
                        "Greska",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning,
                        MessageBoxResult.OK);
                }
                else if (odabrani == null || odabrani.Rows.Count < 1)
                {
                    MessageBox.Show(
                        "Morate odabrati bar jedan proizvod",
                        "Nema odabranih proizvoda",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning,
                        MessageBoxResult.OK);
                }
                else
                {
                    try
                    {
                        ds.Tables[0].Clear();
                        daProizvodi.Fill(ds, "proizvodi");
                    }
                    catch (SqlCeException sqlcexcFak)
                    {
                        MessageBox.Show(
                                sqlcexcFak.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlcexcFak.Message + Environment.NewLine + Environment.NewLine + sqlcexcFak.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }
                    catch (Exception excFak)
                    {
                        MessageBox.Show(
                                excFak.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + excFak.Message + Environment.NewLine + Environment.NewLine + excFak.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }

                    foreach (DataRow kupFak in odabrani.Rows)
                    {
                        flag = 19;
                        foreach (DataRow kupFakOrig in ds.Tables[0].Rows)
                        {
                            if ((kupFak["sifra"].ToString() == kupFakOrig["sifra"].ToString()) && (Double.Parse(kupFak["kol"].ToString()) <= Double.Parse(kupFakOrig["kolicina"].ToString())))
                            {
                                flag = 29;
                            }
                        }
                        if (flag == 19)
                        {
                            MessageBox.Show(
                                "Nema svih proizvoda dovoljno na stanju",
                                "Nema dovoljno",
                                MessageBoxButton.OK,
                                MessageBoxImage.Stop,
                                MessageBoxResult.OK);
                            break;
                        }
                    }
                    if (flag == 29)
                    {
                        foreach (DataRow infoKup in ds.Tables[1].Rows)
                        {
                            if (infoKup["ime"].ToString() == labelOdK.Content.ToString())
                            {
                                mestoKup = infoKup["mesto"].ToString();
                                adresaFak = infoKup["adresa"].ToString() + ", " + infoKup["mesto"];
                                racunFak = infoKup["racun"].ToString();
                                pibFak = infoKup["pib"].ToString();
                            }
                        }

                        string imeFirme = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"> <title>Faktura</title> <style> td {border-left: 3px solid white; border-right: 3px solid white; border-top: 2px dashed gray; text-align: center;} @media print {input {border: none;}} </style> </head>	<body> <div style=\"float: left; line-height: 30%;\"> <h2>JD DELOVI doo</h2> <p>Doža Đerđa 19, 21000 Novi Sad</p> <p>PIB: 107724378</p> <p>Žiro račun: <b>160-376878-74</b></p> <p>Telefon/Fax: 021.6310.565</p> <p>e-mail: jd.delovi@gmail.com</p> </div>";
                        string imeKupca = String.Format("<div style=\"float: right; line-height: 30%; margin-top: 35px;\"> <h2>{0}</h2> <p>{1}</p> <p>PIB: {2}</p> </div> <hr style=\"width: 99%; border: 1px solid white;\" /> <p style=\"margin-left: 15px; margin-top: 30px;\"><b>RAČUN-OTPREMNICA br. <input type=\"text\" style=\"width: 80px; font-weight: bold;\"></b></p> <p>Mesto i datum izdavanja računa: <input type=\"text\" value=\"21000 NOVI SAD, {3}.\" style=\"width: 400px\" > <br /> Mesto i datum prometa dobara: <input type=\"text\" value=\"21000 NOVI SAD, {3}.\" style=\"width: 400px\" > <p> <hr style=\"width: 99%;\" /> <div> <table style=\"font-size: 80%;\"> <tr> <th>Šifra</th> <th>Naziv</th> <th>Cena(kom)</th> <th>Kol</th> <th>Jedinica</th> <th>Iznos</th> <th>Rabat</th> <th>Osnovica</th> </tr>", labelOdK.Content.ToString(), adresaFak, pibFak, System.DateTime.Today.ToShortDateString());

                        osnovica = 0;
                        svega = 0;
                        rabatUk = 0;
                        foreach (DataRow rowOsn in odabrani.Rows)
                        {
                            osnovica += Decimal.Parse(rowOsn["cenaTotal"].ToString());
                            svega += Decimal.Parse(rowOsn["cenaBez"].ToString());

                            rabatProc = 0;
                            prRab = Decimal.TryParse(rowOsn["rabat"].ToString().TrimEnd('%'), out rabatProc);
                            if (prRab == true && rabatProc != 0)
                            {
                                rabatKol = Decimal.Parse(rowOsn["kol"].ToString());
                                rabatUk += Decimal.Parse(rowOsn["cena"].ToString()) * rabatKol / 100 * rabatProc;
                            }
                        }

                        decimal pdvFak = osnovica / 100 * PDVx;

                        decimal total = osnovica + pdvFak;

                        string donje = String.Format("</table> <hr style=\"width: 99%;\"/> <div style=\"float: right; width: 200px; font-size: 80%;\"> <p style=\"float: right; line-height: 30%;\">{3}</p> <p style=\"line-height: 30%;\">Svega:</p> <p style = \"float: right; margin: 0px; line-height: 30%;\">{4}</p> <p style=\"line-height: 30%;\">Rabat:</p> <hr style=\"width: 99%; border: 1px dashed gray;\" /> <p style=\"float: right; margin: 0px;\">{0}</p> <p style=\"line-height: 30%;\">Poreska osnovica:</p> <p style=\"float: right; margin: 0px; line-height: 30%;\">{1}</p> <p style=\"line-height: 30%;\">PDV:</p> <hr style=\"width: 99%; border: 1px dashed gray;\" /> <p style=\"float: right; margin: 0px;\"><b>{2}</b></p> <p style=\"margin: 0px;\"><b>ZA UPLATU:</b></p> </div> <hr style=\"width: 99%; border: 1px solid white;\" /> <div style=\"margin-top: 40px;\"> <p><b>Slovima: <input type=\"text\" style=\"width: 350px; font-weight: bold;\" ></b></p> <p><b>Datum valute plaćanja: <input type=\"text\" style=\"width: 100px; font-weight: bold;\" ></b><p> </div> <p style=\"font-size: 90%; margin-bottom: 70px;\"> Za neblagovremeno plaćanje zaračunavamo zakonsku zateznu kamatu. <br /> U slučaju spora nadležan je Privredni sud u Novom Sadu. </p> </div> <p style=\"float: left; margin-left: 30px;\">Fakturisao:</p> <p style=\"float: right; margin-right: 30px; margin-top: 0px;\">Potpis ovlašćenog lica:</p> </body> </html>", osnovica.ToString("F"), pdvFak.ToString("F"), total.ToString("F"), svega.ToString("F"), rabatUk.ToString("F"));

                        if (File.Exists(putanja + @"\Lager lista files\faktura.html"))
                        {
                            File.Delete(putanja + @"\Lager lista files\faktura.html");
                        }

                        try
                        {
                            foreach (DataRow skidanjeRow in odabrani.Rows)
                            {
                                foreach (DataRow skidanjeOrig in ds.Tables[0].Rows)
                                {
                                    if (skidanjeRow["sifra"].ToString() == skidanjeOrig["sifra"].ToString())
                                    {
                                        skidanjeOrig["kolicina"] = Double.Parse(skidanjeOrig["kolicina"].ToString()) - Double.Parse(skidanjeRow["kol"].ToString());

                                        //ovde pocinje
                                        DataRow izlaziRow = ds.Tables[2].NewRow();
                                        izlaziRow["datum"] = System.DateTime.Today.ToShortDateString();
                                        izlaziRow["firma"] = labelOdK.Content.ToString();
                                        izlaziRow["mesto"] = mestoKup;
                                        izlaziRow["ulaz"] = DBNull.Value;
                                        izlaziRow["izlaz"] = Double.Parse(skidanjeRow["kol"].ToString());
                                        izlaziRow["cena"] = Decimal.Parse(skidanjeRow["cena"].ToString());
                                        if (Decimal.Parse(skidanjeRow["rabat"].ToString().TrimEnd('%')) == 0)
                                        {
                                            izlaziRow["rabat"] = DBNull.Value;
                                        }
                                        else
                                        {
                                            izlaziRow["rabat"] = skidanjeRow["rabat"];
                                        }
                                        izlaziRow["zaliha"] = Double.Parse(skidanjeOrig["kolicina"].ToString());
                                        izlaziRow["sifraPr"] = skidanjeRow["sifra"].ToString().Trim();
                                        ds.Tables[2].Rows.Add(izlaziRow);
                                        daUlazi.Update(ds.Tables[2]);
                                        //ovde zavrsava
                                    }
                                }
                            }

                            daProizvodi.Update(ds.Tables[0]);

                            StreamWriter faktura = new StreamWriter(putanja + @"\Lager lista files\faktura.html", true);
                            faktura.Write(imeFirme);
                            faktura.Write(imeKupca);

                            foreach (DataRow infoPro in odabrani.Rows)
                            {
                                decimal cenacenabez = Decimal.Parse(infoPro["cenabez"].ToString());
                                string redTabele = String.Format("<tr> <td>{0}</td> <td>{1}</td> <td>{4}<label>din</label></td> <td>{2}</td> <td>{3}</td> <td>{7}<label>din</label></td> <td>{6}<label></label></td> <td>{5}<label>din</label></td> </tr>", infoPro["sifra"], infoPro["naziv"], infoPro["kol"], infoPro["jedinica"], infoPro["cena"], infoPro["cenaTotal"], infoPro["rabat"], cenacenabez.ToString("F"));
                                faktura.Write(redTabele);
                            }

                            faktura.Write(donje);
                            faktura.Close();

                            flag = 0;

                            odabrani.Clear();
                            labelOdK.Content = "";
                            chosenK.Text = "";
                            chosenPDV.Text = "";

                            System.Diagnostics.Process.Start(putanja + @"\Lager lista files\faktura.html");
                        }
                        catch (Exception efakt)
                        {
                            MessageBox.Show(
                                efakt.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + efakt.Message + Environment.NewLine + Environment.NewLine + efakt.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }
                    }
                }
                if (aktivniP.Tables.Count > 0)
                {
                    aktivniP.Tables[0].Clear();
                }
                daAktivni.Fill(aktivniP, "Aktivni");
            }
        }

        private void proIzmenipro_Click(object sender, RoutedEventArgs e)
        {
            if (sifraIzmeni.Content == null)
            {
                MessageBox.Show(
                    "Nije odabran nijedan proizvod",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK);
            }
            else if (String.IsNullOrWhiteSpace(sifraIzmeni.Content.ToString()))
            {
                MessageBox.Show(
                    "Nije odabran nijedan proizvod",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK);
            }
            else if (String.IsNullOrWhiteSpace(nazivIzmeni.Text) || String.IsNullOrWhiteSpace(cenaIzmeni.Text))
            {
                MessageBox.Show(
                    "Polja \"naziv\" i \"cena\" ne smeju biti prazni",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK);
            }
            else
            {
                string provera17 = String.Format(" SIFRA: {0}, \n NAZIV: {1}, \n KOLICINA: {2}, \n JEDINICA: {3}, \n CENA: {4} \n", sifraIzmeni.Content.ToString(), nazivIzmeni.Text, kolicinaIzmeni.Content.ToString(), jedinicaIzmeni.Content.ToString(), cenaIzmeni.Text);

                MessageBoxResult izmeneKey = MessageBox.Show(
                    "Unesi izmene? \n \n" + provera17,
                    "Provera",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);

                if (izmeneKey == MessageBoxResult.Yes)
                {
                    string sifraZaNaci = sifraIzmeni.Content.ToString();
                    string nazivZaIzmeniti = nazivIzmeni.Text;
                    decimal cenaZaIzmeniti = 0;
                    bool pl = Decimal.TryParse(cenaIzmeni.Text, out cenaZaIzmeniti);
                    if (pl != true)
                    {
                        MessageBox.Show(
                            "Greska u unetoj ceni",
                            "Greska",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning,
                            MessageBoxResult.OK);
                    }
                    else
                    {
                        try
                        {
                            ds.Tables[0].Clear();
                            daProizvodi.Fill(ds, "proizvodi");
                        }
                        catch (SqlCeException sqlcexcIzm)
                        {
                            MessageBox.Show(
                                    sqlcexcIzm.Message,
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error,
                                    MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlcexcIzm.Message + Environment.NewLine + Environment.NewLine + sqlcexcIzm.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }
                        catch (Exception excIzm)
                        {
                            MessageBox.Show(
                                    excIzm.Message,
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error,
                                    MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + excIzm.Message + Environment.NewLine + Environment.NewLine + excIzm.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }

                        try
                        {
                            foreach (DataRow rowIzmenjen in ds.Tables[0].Rows)
                            {
                                if ((string)rowIzmenjen["sifra"] == sifraZaNaci)
                                {
                                    rowIzmenjen["ime"] = nazivZaIzmeniti;
                                    rowIzmenjen["cena"] = cenaZaIzmeniti;
                                    daProizvodi.Update(ds.Tables[0]);
                                    break;
                                }
                            }
                            sifraIzmeni.Content = "";
                            nazivIzmeni.Text = "";
                            kolicinaIzmeni.Content = "";
                            jedinicaIzmeni.Content = "";
                            cenaIzmeni.Text = "";
                        }
                        catch (SqlCeException sqlexcIzmF)
                        {
                            MessageBox.Show(
                                    sqlexcIzmF.Message,
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error,
                                    MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + sqlexcIzmF.Message + Environment.NewLine + Environment.NewLine + sqlexcIzmF.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }
                        catch (Exception excIzmF)
                        {
                            MessageBox.Show(
                                    excIzmF.Message,
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error,
                                    MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** " + System.DateTime.Now.ToString() + Environment.NewLine + excIzmF.Message + Environment.NewLine + Environment.NewLine + excIzmF.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }
                    }

                    if (aktivniP.Tables.Count > 0)
                    {
                        aktivniP.Tables[0].Clear();
                    }
                    daAktivni.Fill(aktivniP, "Aktivni");
                }
            }
        }

        private void datagridP_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datagridP.SelectedItem != null)
            {
                DataRowView izmeniProi = (DataRowView)datagridP.SelectedItem;

                string sifraPrvobitna = izmeniProi.Row.ItemArray[1].ToString();
                string imePrvobitno = izmeniProi.Row.ItemArray[2].ToString();
                string kolPrvobitna = izmeniProi.Row.ItemArray[3].ToString();
                string jedPrvobitna = izmeniProi.Row.ItemArray[4].ToString();
                string cenaPrvobitna = izmeniProi.Row.ItemArray[5].ToString();

                if (sifraPrvobitna != null)
                {
                    sifraIzmeni.Content = sifraPrvobitna;
                }
                else
                {
                    sifraIzmeni.Content = "****NEMA****";
                }

                if (imePrvobitno != null)
                {
                    nazivIzmeni.Text = imePrvobitno;
                }
                else
                {
                    nazivIzmeni.Text = "****NEMA****";
                }

                if (kolPrvobitna != null)
                {
                    kolicinaIzmeni.Content = kolPrvobitna;
                }
                else
                {
                    kolicinaIzmeni.Content = "****NEMA****";
                }

                if (jedPrvobitna != null)
                {
                    jedinicaIzmeni.Content = jedPrvobitna;
                }
                else
                {
                    jedinicaIzmeni.Content = "****NEMA****";
                }

                if (cenaPrvobitna != null)
                {
                    cenaIzmeni.Text = cenaPrvobitna;
                }
                else
                {
                    cenaIzmeni.Text = "****NEMA****";
                }
            }
        }

        private void proIzbrisipro_Click(object sender, RoutedEventArgs e)
        {
            if (sifraIzmeni.Content == null)
            {
                MessageBox.Show(
                    "Nije odabran nijedan proizvod",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else if (String.IsNullOrWhiteSpace(sifraIzmeni.Content.ToString()))
            {
                MessageBox.Show(
                    "Nije odabran nijedan proizvod",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else
            {
                MessageBoxResult keyBrisi = MessageBox.Show(
                    "Da li ste sigurni da zelite da obrisete odabrani proizvod?",
                    "Obrisi?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);
                if (keyBrisi == MessageBoxResult.Yes)
                {
                    string sifraBrisi = sifraIzmeni.Content.ToString();
                    try
                    {
                        ds.Tables[0].Clear();
                        daProizvodi.Fill(ds, "proizvodi");

                        foreach (DataRow rowBrisi in ds.Tables[0].Rows)
                        {
                            if ((string)rowBrisi["sifra"] == sifraBrisi)
                            {
                                rowBrisi.Delete();
                                daProizvodi.Update(ds.Tables[0]);
                                break;
                            }
                        }

                        SqlCeCommand brisIkart = new SqlCeCommand();
                        brisIkart.Connection = cn;
                        brisIkart.CommandType = CommandType.Text;
                        brisIkart.CommandText = String.Format("DELETE FROM ulazi WHERE sifraPr = '{0}'", sifraBrisi.Trim());
                        brisIkart.ExecuteNonQuery();

                        ui.Tables[0].Clear();

                        sifraIzmeni.Content = "";
                        nazivIzmeni.Text = "";
                        kolicinaIzmeni.Content = "";
                        jedinicaIzmeni.Content = "";
                        cenaIzmeni.Text = "";
                    }
                    catch (SqlCeException sqlBrisi)
                    {
                        MessageBox.Show(
                            sqlBrisi.Message,
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + sqlBrisi.Message + Environment.NewLine + Environment.NewLine + sqlBrisi.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }
                    catch (Exception excBrisi)
                    {
                        MessageBox.Show(
                            excBrisi.Message,
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);

                        FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter fssw = new StreamWriter(fs);
                        fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + excBrisi.Message + Environment.NewLine + Environment.NewLine + excBrisi.StackTrace + Environment.NewLine);
                        fssw.Close();
                    }
                    if (aktivniP.Tables.Count > 0)
                    {
                        aktivniP.Tables[0].Clear();
                    }
                    daAktivni.Fill(aktivniP, "Aktivni");
                }
            }
        }

        private void prikKalk_Click(object sender, RoutedEventArgs e)
        {
            if (sifraIzmeni.Content == null)
            {
                MessageBox.Show(
                    "Nije odabran nijedan proizvod!",
                    "Odaberite proizvod",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else
            {
                if (gridKartIzm.Visibility == Visibility.Visible)
                {
                    gridKartIzm.Visibility = Visibility.Hidden;
                }

                ui.Tables[0].Clear();
                string ulaziNaredba = String.Format("SELECT * FROM ulazi WHERE sifraPr = '{0}'", sifraIzmeni.Content.ToString().Trim());
                daUlaziSpec = new SqlCeDataAdapter(ulaziNaredba, cn);
                daUlaziSpec.Fill(ui, "in");

                proizvodKalk.Content = nazivIzmeni.Text;
                sifraKalk.Content = sifraIzmeni.Content.ToString().Trim();

                if (datagridUlazi.Columns.Count > 9)
                {
                    datagridUlazi.Columns[0].Visibility = Visibility.Hidden;
                    datagridUlazi.Columns[9].Visibility = Visibility.Hidden;
                }

                kalkulacije.IsSelected = true;
            }
        }

        private void resetUlaze_Click(object sender, RoutedEventArgs e)
        {
            if (ui.Tables.Count > 0)
            {
                if (ui.Tables[0].Rows.Count > 0)
                {
                    MessageBoxResult brisrez = MessageBox.Show(
                        "Da li ste sigurni da zelite da resetujete kalkulacije za dati proizvod?",
                        "Resetuj?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes);

                    if (brisrez == MessageBoxResult.Yes)
                    {
                        SqlCeCommand cmdbris = new SqlCeCommand();
                        cmdbris.Connection = cn;
                        cmdbris.CommandType = CommandType.Text;
                        cmdbris.CommandText = String.Format("DELETE FROM ulazi WHERE sifraPr = '{0}'", sifraKalk.Content);
                        cmdbris.ExecuteNonQuery();

                        ui.Tables[0].Clear();
                        gridKartIzm.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void datagridUlazi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datagridUlazi.SelectedItem != null)
            {
                if (gridKartIzm.Visibility == Visibility.Hidden)
                {
                    gridKartIzm.Visibility = Visibility.Visible;
                }

                DataRowView izmeniUl = (DataRowView)datagridUlazi.SelectedItem;

                if (izmeniUl.Row.ItemArray[0] != null)
                {
                    karticaId.Content = izmeniUl.Row.ItemArray[0].ToString();
                }
                else
                {
                    karticaId.Content = "";
                }

                if (izmeniUl.Row.ItemArray[9] != null)
                {
                    karticaSif.Content = izmeniUl.Row.ItemArray[9].ToString();
                }
                else
                {
                    karticaSif.Content = "";
                }

                if (izmeniUl.Row.ItemArray[1] != null)
                {
                    karticaDatum.Text = izmeniUl.Row.ItemArray[1].ToString();
                }
                else
                {
                    karticaDatum.Text = "";
                }

                if (izmeniUl.Row.ItemArray[2] != null)
                {
                    karticaFirma.Text = izmeniUl.Row.ItemArray[2].ToString();
                }
                else
                {
                    karticaFirma.Text = "";
                }

                if (izmeniUl.Row.ItemArray[3] != null)
                {
                    karticaMesto.Text = izmeniUl.Row.ItemArray[3].ToString();
                }
                else
                {
                    karticaMesto.Text = "";
                }

                if (izmeniUl.Row.ItemArray[4] != null && !String.IsNullOrWhiteSpace(izmeniUl.Row.ItemArray[4].ToString()))
                {
                    karticaUI.Content = "ulaz:";
                    ulazIzlaz.Text = izmeniUl.Row.ItemArray[4].ToString();
                }
                else if (izmeniUl.Row.ItemArray[5] != null && !String.IsNullOrWhiteSpace(izmeniUl.Row.ItemArray[5].ToString()))
                {
                    karticaUI.Content = "izlaz:";
                    ulazIzlaz.Text = izmeniUl.Row.ItemArray[5].ToString();
                }
                else
                {
                    karticaUI.Content = "ulaz/izlaz:";
                    ulazIzlaz.Text = "";
                }

                if (izmeniUl.Row.ItemArray[6] != null)
                {
                    karticaCena.Text = izmeniUl.Row.ItemArray[6].ToString();
                }
                else
                {
                    karticaCena.Text = "";
                }

                if (izmeniUl.Row.ItemArray[7] != null && !String.IsNullOrWhiteSpace(izmeniUl.Row.ItemArray[7].ToString()))
                {
                    karticaRabat.Text = izmeniUl.Row.ItemArray[7].ToString();
                }
                else
                {
                    karticaRabat.Text = "";
                }

                if (izmeniUl.Row.ItemArray[8] != null)
                {
                    karticaZaliha.Text = izmeniUl.Row.ItemArray[8].ToString();
                }
                else
                {
                    karticaZaliha.Text = "";
                }
            }
        }

        private void karticaPonisti_Click(object sender, RoutedEventArgs e)
        {
            gridKartIzm.Visibility = Visibility.Hidden;
        }

        private void karticaIzmeni_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(karticaDatum.Text) || String.IsNullOrWhiteSpace(karticaFirma.Text) || String.IsNullOrWhiteSpace(karticaMesto.Text) || String.IsNullOrWhiteSpace(karticaCena.Text) || String.IsNullOrWhiteSpace(karticaZaliha.Text))
            {
                MessageBox.Show(
                    "Niste popunili neka od obaveznih polja.",
                    "Prazna polja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else if (karticaId.Content == null)
            {
                MessageBox.Show(
                    "Nije odabrana nijedna stavka!",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else
            {
                int idm2;
                bool izmStav = Int32.TryParse(karticaId.Content.ToString(), out idm2);
                if (izmStav != true)
                {
                    MessageBox.Show(
                        "Nije odabrana nijedna stavka.",
                        "Greska",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);
                }
                else
                {
                    decimal izmStavCena;
                    int izmStavZaliha;
                    bool izmCenab, izmZalihab;

                    izmCenab = Decimal.TryParse(karticaCena.Text, out izmStavCena);
                    izmZalihab = Int32.TryParse(karticaZaliha.Text, out izmStavZaliha);

                    if (izmCenab != true)
                    {
                        MessageBox.Show(
                            "Greska u unetoj ceni",
                            "Neispravan unos",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);
                    }
                    else if (izmZalihab != true)
                    {
                        MessageBox.Show(
                            "Greska u unetoj zalihi",
                            "Neispravan unos",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);
                    }

                    else
                    {
                        MessageBoxResult izmStavkey = MessageBox.Show(
                            "Da li ste sigurni da zelite da izmenite odabranu stavku?",
                            "Izmeniti?",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question,
                            MessageBoxResult.Yes);

                        if (izmStavkey == MessageBoxResult.Yes)
                        {
                            string stavIzmUpit;
                            string cenaEscaped = izmStavCena.ToString().Replace(',', '.');
                            if (karticaUI.Content.ToString().Trim() == "ulaz:")
                            {
                                stavIzmUpit = String.Format("UPDATE ulazi SET datum='{0}', firma='{1}', mesto='{2}', ulaz='{3}', cena={4}, zaliha={5} WHERE id={6}", karticaDatum.Text, karticaFirma.Text, karticaMesto.Text, ulazIzlaz.Text, cenaEscaped, izmStavZaliha, idm2);
                            }
                            else
                            {
                                stavIzmUpit = String.Format("UPDATE ulazi SET datum='{0}', firma='{1}', mesto='{2}', izlaz='{3}', cena={4}, rabat='{5}', zaliha={6} WHERE id={7}", karticaDatum.Text, karticaFirma.Text, karticaMesto.Text, ulazIzlaz.Text, cenaEscaped, karticaRabat.Text, izmStavZaliha, idm2);
                            }

                            try
                            {
                                SqlCeCommand izmStavcmd = new SqlCeCommand();
                                izmStavcmd.Connection = cn;
                                izmStavcmd.CommandType = CommandType.Text;
                                izmStavcmd.CommandText = stavIzmUpit;
                                izmStavcmd.ExecuteNonQuery();

                                ui.Tables[0].Clear();
                                string ulaziNaredba3 = String.Format("SELECT * FROM ulazi WHERE sifraPr = '{0}'", sifraKalk.Content.ToString());
                                daUlaziSpec = new SqlCeDataAdapter(ulaziNaredba3, cn);
                                daUlaziSpec.Fill(ui, "in");

                                gridKartIzm.Visibility = Visibility.Hidden;
                            }
                            catch (SqlCeException izmStavSqlExc)
                            {
                                MessageBox.Show(
                                izmStavSqlExc.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                                FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                                StreamWriter fssw = new StreamWriter(fs);
                                fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + izmStavSqlExc.Message + Environment.NewLine + Environment.NewLine + izmStavSqlExc.StackTrace + Environment.NewLine);
                                fssw.Close();
                            }
                            catch (Exception izmStavExc)
                            {
                                MessageBox.Show(
                                izmStavExc.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK);

                                FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                                StreamWriter fssw = new StreamWriter(fs);
                                fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + izmStavExc.Message + Environment.NewLine + Environment.NewLine + izmStavExc.StackTrace + Environment.NewLine);
                                fssw.Close();
                            }
                        }
                    }
                }
            }
        }

        private void karticaObrisi_Click(object sender, RoutedEventArgs e)
        {
            if (karticaId.Content == null)
            {
                MessageBox.Show(
                    "Nije odabrana nijedna stavka.",
                    "Greska",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
            else
            {
                int idm;
                bool brisStav = Int32.TryParse(karticaId.Content.ToString(), out idm);
                if (brisStav != true)
                {
                    MessageBox.Show(
                        "Nije odabrana nijedna stavka.",
                        "Greska",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);
                }
                else
                {
                    MessageBoxResult obrStavKart = MessageBox.Show(
                        "Da li ste sigurni da zelite da obrisete odabrani stavku?",
                        "Obrisati?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No);
                    if (obrStavKart == MessageBoxResult.Yes)
                    {
                        string brisStavUpit = String.Format("DELETE FROM ulazi WHERE id={0}", idm);

                        try
                        {
                            SqlCeCommand brisStavcmd = new SqlCeCommand();
                            brisStavcmd.Connection = cn;
                            brisStavcmd.CommandType = CommandType.Text;
                            brisStavcmd.CommandText = brisStavUpit;
                            brisStavcmd.ExecuteNonQuery();

                            ui.Tables[0].Clear();
                            string ulaziNaredba2 = String.Format("SELECT * FROM ulazi WHERE sifraPr = '{0}'", sifraKalk.Content.ToString());
                            daUlaziSpec = new SqlCeDataAdapter(ulaziNaredba2, cn);
                            daUlaziSpec.Fill(ui, "in");

                            gridKartIzm.Visibility = Visibility.Hidden;
                        }
                        catch (SqlCeException brisStavSqlExc)
                        {
                            MessageBox.Show(
                            brisStavSqlExc.Message,
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + brisStavSqlExc.Message + Environment.NewLine + Environment.NewLine + brisStavSqlExc.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }
                        catch (Exception brisStavExc)
                        {
                            MessageBox.Show(
                            brisStavExc.Message,
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);

                            FileStream fs = new FileStream(putanja + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
                            StreamWriter fssw = new StreamWriter(fs);
                            fssw.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + brisStavExc.Message + Environment.NewLine + Environment.NewLine + brisStavExc.StackTrace + Environment.NewLine);
                            fssw.Close();
                        }
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult key = MessageBox.Show(
                "Da li ste sigurni da zelite da izadjete?",
                "Potvrdi izlazak",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);
            if (key == MessageBoxResult.Yes)
            {
                cn.Close();
            }
            e.Cancel = (key == MessageBoxResult.No);
        }
                                                                                                                                                                                                                                                                                                  
    }
}
