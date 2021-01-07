using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Threading;

namespace IRI_konwerter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public delegate void KonwertujDelegate(string rodzWarstw, string nrJezd, string pasRuch, string rodzPasa, DaneProbki daneProbki, string nzPlikForm);
    public delegate void przypiszTekstDelegate(string x);
    public delegate void UdostepnijKonwButtonDelegate();

    public partial class MainWindow : Window
    {
        #region deklaracja zmiennych

        string sciezkaPliku = "", sciezkaProgramu = "", data = "", labownikPath = "";
        int iloscArkuszy = 0, firstRowToFillInLab = 4;
        Excel.Application app;
        Excel.Workbook skoroszytZrodlowy;
        Excel.Workbook skoroszytSzablon, skoroszytTemp, labownikBook;
        Excel.Worksheet arkuszZrodlowy, arkuszTemp, arkuszSzablon, labownikSheet;
        IRI iri = new IRI();
        przypiszTekstDelegate przypiszTekstDel;
        KonwertujDelegate konwertujDel;
        IAsyncResult konwertujAsRes;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            app = new Excel.Application();
            app.Visible = false;
            app.DisplayAlerts = false;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            try
            {
                if(konwertujDel != null)
                konwertujDel.EndInvoke(konwertujAsRes);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            try
            {
                skoroszytZrodlowy.Close(false, Missing.Value, Missing.Value);
                skoroszytTemp.Close(true, Missing.Value, Missing.Value);
                skoroszytSzablon.Close(false, Missing.Value, Missing.Value);
            }
            catch { }
            try
            {
                labownikBook.Save();
                labownikBook.Close(true, Missing.Value, Missing.Value);
            }
            catch { }
            if (app!=null)
                app.Quit();
            releaseObject(skoroszytZrodlowy);
            releaseObject(skoroszytTemp);
            releaseObject(skoroszytSzablon);
            releaseObject(labownikBook);
            releaseObject(labownikSheet);
            releaseObject(arkuszTemp);
            releaseObject(arkuszZrodlowy);
            releaseObject(app);
            }

        private void button_plikZrodlowy_Click(object sender, RoutedEventArgs e)
        {
            sciezkaPliku = GetPath();
            if (sciezkaPliku != null && sciezkaPliku != "")
            {
                OtworzPlikZrodlowy();
                textBox_sciezkaIRI.Text = sciezkaPliku;
                UtworzPlikTymczasowy();
                WyswietlDaneProbkiwForm();
                try
                {
                    PobierzWyniki();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Wystąpił błąd, sprawdź czy podany plik jest prawidłowy\n\n" + ex.Message, "Błąd!");
                }
            }
        }
        private void OtworzPlikZrodlowy()
        {
            if (app == null)
            {
                app = new Excel.Application();
                app.Visible = false;
                app.DisplayAlerts = false;
            }
            skoroszytZrodlowy = app.Workbooks.Open(sciezkaPliku);
        }
        private void UtworzPlikTymczasowy()
        {
            sciezkaProgramu = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            skoroszytSzablon = app.Workbooks.Open(sciezkaProgramu + "/szablony/IRI_pusty.xlsx");
            if (System.IO.File.Exists(sciezkaProgramu + "/temp/temp.xlsx"))
            {
                System.IO.File.Delete(sciezkaProgramu + "/temp/temp.xlsx");
            }
            skoroszytSzablon.SaveAs(sciezkaProgramu + "/temp/temp.xlsx");
            skoroszytTemp = app.Workbooks.Open(sciezkaProgramu + "/temp/temp.xlsx");
            for (int i = 1; i < skoroszytZrodlowy.Sheets.Count; i++)
            {
                skoroszytSzablon = app.Workbooks.Open(sciezkaProgramu + "/szablony/IRI_pusty.xlsx");
                arkuszSzablon = skoroszytSzablon.Worksheets[1];
                arkuszSzablon.Copy(skoroszytTemp.Worksheets[1]);
                skoroszytSzablon.Close(false);
            }
            for (int j = 1; j <= skoroszytTemp.Sheets.Count; j++)
            {
                arkuszTemp = skoroszytTemp.Sheets[j];
                arkuszTemp.Name = "km " + j;
            }
        }
        private void ZapiszPlikWynikowy(string nazwaPlikuForm)
        {
            data = System.DateTime.Now.ToString();
            data = Regex.Replace(data, "-", "_");
            data = Regex.Replace(data, ":", "");
            string nazwaPliku = "wynik " + data;
            if (nazwaPlikuForm != null && nazwaPlikuForm != "") { nazwaPliku = nazwaPlikuForm; }
            else if (iri != null && iri.daneProbki.nrZlecenia != null && iri.daneProbki.nrZlecenia != "") { nazwaPliku = iri.daneProbki.nrZlecenia; }
            else { nazwaPliku = "wynik " + data; }
            if (skoroszytTemp != null)
            {
                skoroszytTemp.SaveAs(sciezkaProgramu + "/wyniki/" + nazwaPliku + ".xlsx");
            }
            if (System.IO.File.Exists(sciezkaProgramu + "/temp/temp.xlsx"))
            {
                System.IO.File.Delete(sciezkaProgramu + "/temp/temp.xlsx");
            }
        }
        private void PobierzWyniki()
        {
            WynikIRI wynikIRI = new WynikIRI();
            int liczbaWynikow = 0;
            iloscArkuszy = skoroszytZrodlowy.Sheets.Count;
            for (int nrArk = 1; nrArk <= iloscArkuszy; nrArk++)
            {
                //pobiera wyniki z kilometrażem
                arkuszZrodlowy = skoroszytZrodlowy.Sheets[nrArk];
                object[,] tablica = arkuszZrodlowy.get_Range("A13", "E38").Value;
                int dl = tablica.Length / 5;
                //TextBoxInfo.Text += tablica[5, 5].ToString() + "\n";
                for (int asdf = 1; asdf <= dl; asdf++)
                {
                    if (tablica[asdf, 1] != null)
                    {
                        wynikIRI = new WynikIRI();
                        wynikIRI.kilometr = tablica[asdf, 4].ToString();
                        wynikIRI.kilometr.Replace(".", ",");
                        wynikIRI.iri = tablica[asdf, 5].ToString();
                        wynikIRI.iri.Replace(".", ",");
                        wynikIRI.iriRosnace = tablica[asdf, 2].ToString();
                        wynikIRI.iriRosnace.Replace(".", ",");
                        wynikIRI.punkt = tablica[asdf, 1].ToString();
                        wynikIRI.punkt.Replace(".", ",");
                        iri.wyniki.Add(wynikIRI);
                        liczbaWynikow++;
                    }
                }
                tablica = null;
                //pobiera wyniki z tabelki
                object[,] tablicaIri = arkuszZrodlowy.get_Range("N8", "P11").Value;
                iri.iri1.Add(tablicaIri[1, 1].ToString());
                iri.iri2.Add(tablicaIri[1, 2].ToString());
                iri.iri3.Add(tablicaIri[1, 3].ToString());
                iri.iriWym1.Add(tablicaIri[4, 1].ToString());
                iri.iriWym2.Add(tablicaIri[4, 2].ToString());
                iri.iriWym3.Add(tablicaIri[4, 3].ToString());
                tablicaIri = null;
            }
        }
        private void WyswietlDaneProbkiwForm()
        {
            arkuszZrodlowy = skoroszytZrodlowy.Sheets[1];

            #region droga
            string drogaIRI = "";
            if (arkuszZrodlowy.Cells[2, "A"].Value != null)
            {
                drogaIRI = arkuszZrodlowy.Cells[2, "A"].Value.ToString();
                drogaIRI = drogaIRI.Substring(0, drogaIRI.IndexOf("km")).Trim();
            }
            else { drogaIRI = ""; }
            budowaDrogaTextBox.Text = drogaIRI;
            #endregion
            #region odcinek
            string odcinekIRI = "";
            if (arkuszZrodlowy.Cells[8, "K"].Value != null)
            {
                odcinekIRI = arkuszZrodlowy.Cells[8, "K"].Value.ToString();
                if (odcinekIRI.Contains("km"))
                    odcinekIRI = odcinekIRI.Substring(odcinekIRI.IndexOf("km"));
                odcinekIRI = odcinekIRI.Trim();
            }
            else { odcinekIRI = ""; }
            odcinekEtapBox.Text = odcinekIRI;
            #endregion
            #region miejsce pomiaru
            string miejscePomiaruIRI = "";
            if (arkuszZrodlowy.Cells[8, "K"].Value != null)
            {
                if (arkuszZrodlowy.Cells[8, "K"].Value.ToString() == "P") { miejscePomiaruIRI = "strona prawa"; }
                else if (arkuszZrodlowy.Cells[8, "K"].Value.ToString() == "L") { miejscePomiaruIRI = "strona lewa"; }
                else
                {
                    miejscePomiaruIRI = "";
                }
            }
            else { miejscePomiaruIRI = ""; }
            miejscePomiaruTextBox.Text = miejscePomiaruIRI;
            #endregion
            //pobiera dane z tabelki
            #region warstwa
            string warstwaIRI = "";
            if (arkuszZrodlowy.Cells[7, "I"].Value != null)
            {
                warstwaIRI = arkuszZrodlowy.Cells[7, "I"].Value.ToString();
                warstwaIRI = warstwaIRI.Trim();
            }
            else { warstwaIRI = ""; }
            rodzajWarstwyTextBox.Text = warstwaIRI;
            #endregion
            #region rodzaj konstrukcji
            string rodzajKonstrukcjiIRI = "";
            if (arkuszZrodlowy.Cells[7, "I"].Value != null)
            {
                rodzajKonstrukcjiIRI = arkuszZrodlowy.Cells[7, "I"].Value.ToString();
                rodzajKonstrukcjiIRI = rodzajKonstrukcjiIRI.Trim();
            }
            else { rodzajKonstrukcjiIRI = ""; }
            rodzajWarstwyTextBox.Text = rodzajKonstrukcjiIRI;
            #endregion
            #region numer jezdni
            string numerJezdniIRI = "";
            if (arkuszZrodlowy.Cells[6, "K"].Value != null)
            {
                numerJezdniIRI = arkuszZrodlowy.Cells[6, "K"].Value.ToString();
                numerJezdniIRI = numerJezdniIRI.Trim();
            }
            else { numerJezdniIRI = ""; }
            nrJezdniTextBox.Text = numerJezdniIRI;
            #endregion
            #region pas ruchu
            string pasRuchuIRI = "";
            if (arkuszZrodlowy.Cells[8, "K"].Value != null)
            {
                pasRuchuIRI = arkuszZrodlowy.Cells[8, "K"].Value.ToString();
                pasRuchuIRI = pasRuchuIRI.Trim();
            }
            else { pasRuchuIRI = ""; }
            pasRuchuTextBox.Text = pasRuchuIRI;
            #endregion
            #region rodzaj pasa
            string rodzajPasaIRI = "";
            if (arkuszZrodlowy.Cells[10, "K"].Value != null)
            {
                rodzajPasaIRI = arkuszZrodlowy.Cells[10, "K"].Value.ToString();
                rodzajPasaIRI = rodzajPasaIRI.Trim();
            }
            else { rodzajPasaIRI = ""; }
            rodzajNrPasaTextBox.Text = rodzajPasaIRI;
            #endregion
            nrProbkiKlientTextBox.Text = "-";
            wymaganiaTextBox.Text = "Wartość wskaźnika IRI 50% ≤ 1.2, 80% ≤ 2.0, 100% ≤ 3.3";
            wymaganiaWgTextBox.Text = "Rozporządzenia Ministra Transportu i Gospodarki Morskiej z dnia 2 marca 1999 r. w sprawie warunków technicznych, jakim powinny odpowiadać drogi publiczne i ich usytuowanie Dziennik Ustaw Nr. 43 poz 430 z roku 1999";
            
        }

        private void button_labownik_Click(object sender, RoutedEventArgs e)
        {
            labownikPath = GetPath();
            textBox_sciezkaLab.Text = labownikPath;
        }
        private string GetPath()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.ShowDialog();
            return dlg.FileName;
        }

        //konwertuje sprawozdanie
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            iri.PrzeliczKm(kilometrPoczatkowyTextBox.Text);
            DaneProbki pobraneDane = PobierzDaneProbkizForm();
            string nazwaPliku = nazwaPlikuTextBox.Text;
            try
            {
                konwertujDel = new KonwertujDelegate(Konwertuj);
                konwertujAsRes = konwertujDel.BeginInvoke(rodzajWarstwyTextBox.Text, nrJezdniTextBox.Text, pasRuchuTextBox.Text, rodzajNrPasaTextBox.Text, pobraneDane, nazwaPliku, new AsyncCallback(UdostepnijKonwertujButton), null);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            TextBoxInfo.Text += "\nRozpoczęto kopiowanie.";
            konwertujButton.IsEnabled = false;
        }
        public void Konwertuj(string rodzWarstw, string nrJezd, string pasRuch, string rodzPasa, DaneProbki daneProbki, string nzPlikForm)
        {
            int start = System.Environment.TickCount;
            app.Calculation = Excel.XlCalculation.xlCalculationManual;
            PrzeniesWynikiDoSprawozdania(rodzWarstw, nrJezd, pasRuch, rodzPasa);
            PrzeniesDaneProbkiDoSprawozdania(daneProbki);
            app.Calculate();
            ZapiszPlikWynikowy(nzPlikForm);
            app.Calculation = Excel.XlCalculation.xlCalculationAutomatic;
            int end = System.Environment.TickCount;
            int czas = end - start;
            string tekst = "\nKonwertowanie ukończone\nczas trwania operacji: " + czas + " ms";
            przypiszTekstDel = new przypiszTekstDelegate(przypiszTekstDoInfo);
            TextBoxInfo.Dispatcher.Invoke(przypiszTekstDel, tekst);
        }
        public DaneProbki PobierzDaneProbkizForm()
        {
            DaneProbki dane = new DaneProbki();
            dane.budowa = budowaDrogaTextBox.Text;
            dane.data = dataTextBox.Text;
            dane.dataPomiaru = dataPomiaruTextBox.Text;
            dane.droga = budowaDrogaTextBox.Text;
            dane.etap = odcinekEtapBox.Text;
            dane.kilometrPoczatkowy = kilometrPoczatkowyTextBox.Text;
            dane.miejscePomiaru = miejscePomiaruTextBox.Text;
            dane.nrProbki = nrProbkiT1TextBox.Text;
            dane.nrProbkiKlient = nrProbkiKlientTextBox.Text;
            dane.nrZlecenia = nrZleceniaTextBox.Text;
            dane.odcinek = odcinekEtapBox.Text;
            dane.warstwa = warstwaTextBox.Text;
            dane.wymagania = wymaganiaTextBox.Text;
            dane.wymaganiaWg = wymaganiaWgTextBox.Text;
            dane.zleceniodawca = zleceniodawcaTextBox.Text;
            return dane;
        }
        public void PrzeniesDaneProbkiDoSprawozdania(DaneProbki daneProb)
        {
            labownikBook = app.Workbooks.Open(labownikPath);
            labownikSheet = labownikBook.Sheets[1];
            firstRowToFillInLab = 5;
            //firstRowToFillInLab = FindFirstEmptyRowIn(labownikSheet);
            string nrProbkiT1 = "", nrProbkiOgon = "";
            int nrProbkiBezOgona = 1;
            if (daneProb.nrZlecenia != null && daneProb.nrZlecenia != "")
            {
                iri.daneProbki.nrZlecenia = daneProb.nrZlecenia;
                try
                {
                    nrProbkiT1 = daneProb.nrProbki;
                    nrProbkiBezOgona = int.Parse(nrProbkiT1.Substring(0, nrProbkiT1.IndexOf("/")));
                    nrProbkiOgon = nrProbkiT1.Substring(nrProbkiT1.IndexOf("/"));
                }
                catch { }
            }
            string[] daneProbkizForm = {
                        daneProb.data,
                        daneProb.zleceniodawca,
                        daneProb.nrZlecenia,
                        daneProb.budowa,
                        daneProb.etap,
                        nrProbkiT1,
                        daneProb.nrProbkiKlient, //nr próbki wg klienta
                        daneProb.wymaganiaWg,    //wymaganiaWg
                        daneProb.wymagania,
                        daneProb.miejscePomiaru,
                        daneProb.warstwa,
                        daneProb.dataPomiaru
                        };
            for (int idxArkusza = 1; idxArkusza <= iloscArkuszy; idxArkusza++)
            {
                nrProbkiT1 = nrProbkiBezOgona + nrProbkiOgon;
                arkuszTemp = skoroszytTemp.Sheets[idxArkusza];
                int nrKomorki = 0;
                Excel.Range tabelkazDanymiwSpr = arkuszTemp.get_Range("D5", "D17");
                foreach (Excel.Range komorka in tabelkazDanymiwSpr)
                {
                    if (nrKomorki < 8)
                    {
                        if (nrKomorki == 5)
                        {
                            komorka.Value = nrProbkiT1;
                        }
                        else
                        {
                            komorka.Value = daneProbkizForm[nrKomorki];
                        }
                        nrKomorki++;
                    }
                    else
                    {
                        if (nrKomorki == 10)
                        {
                            komorka.Value = daneProbkizForm[nrKomorki - 1] + " od km " + iri.daneProbki.kmOd[idxArkusza - 1] + " do km " + iri.daneProbki.kmDo[idxArkusza - 1];
                        }
                        else
                        {
                            komorka.Value = daneProbkizForm[nrKomorki - 1];
                        }
                        nrKomorki++;
                    }

                }
                nrKomorki = 0;
                arkuszTemp.Cells[2, "A"].Value = "SPRAWOZDANIE NR " + nrProbkiBezOgona + "/ZDN/2014\nZ POMIARÓW RÓWNOŚCI PODŁUŻNEJ NAWIERZCHNI PROFILOGRAFEM LASEROWYM RSP";

                labownikSheet.Cells[firstRowToFillInLab, "A"].Value = daneProb.dataPomiaru;
                labownikSheet.Cells[firstRowToFillInLab, "B"].Value = daneProb.dataPomiaru;
                labownikSheet.Cells[firstRowToFillInLab, "C"].Value = daneProb.nrProbki;
                
                firstRowToFillInLab += 2;
                nrProbkiBezOgona++;
                labownikBook.Save();
            }
            
            daneProbkizForm = null;
        }
        private int FindFirstEmptyRowIn(Excel.Worksheet sheet)
        {
            int i = 5;
            while (sheet.Cells[i, "C"].Value != null || sheet.Cells[i, "C"].Value != "")
            {
                if ((Boolean)sheet.Cells[i, "C"].MergeCells)
                {
                    do { i += 2; }
                    while ((Boolean)sheet.Cells[i, "C"].MergeCells);
                }
                else { i += 2; }
            }
            return i;
        }
        public void PrzeniesWynikiDoSprawozdania(string rodzWarstw, string nrJezd, string pasRuch, string rodzPasa)
        {
            int nrWynik = 0;
            int liczbaWskIRI = 0;
            for (int idxArkusza = 1; idxArkusza <= skoroszytTemp.Sheets.Count; idxArkusza++)
            {
                liczbaWskIRI = 0;
                arkuszTemp = skoroszytTemp.Sheets[idxArkusza];
                //kopiuje same wyniki z kilometrażem
                if (nrWynik >= iri.wyniki.Count) break;
                double km = double.Parse(iri.wyniki[nrWynik].kilometr);
                km -= 0.05;
                string kmOd = km.ToString();
                kmOd = Regex.Replace(kmOd, ",", "+");
                iri.daneProbki.kmOd.Add(kmOd);
                double[] kmTab = new double[20];
                double[] iriTab = new double[20];
                double[] pktTab = new double[20];
                double[] iriRosTab = new double[20];
                for (int idx = 0; idx < 20; idx++)
                {
                    if (nrWynik >= iri.wyniki.Count)
                    {
                        kmTab[idx] = 0;
                        iriTab[idx] = 0;
                        pktTab[idx] = 0;
                        iriRosTab[idx] = 0;
                    }
                    else
                    {
                        kmTab[idx] = double.Parse(iri.wyniki[nrWynik].kilometr);
                        iriTab[idx] = double.Parse(iri.wyniki[nrWynik].iri);
                        pktTab[idx] = double.Parse(iri.wyniki[nrWynik].punkt);
                        iriRosTab[idx] = double.Parse(iri.wyniki[nrWynik].iriRosnace);
                        liczbaWskIRI++;
                        nrWynik++;
                    }
                }
                int index = 0;
                Excel.Range komRng = arkuszTemp.get_Range("A20", "A39");
                foreach (Excel.Range x in komRng)
                {
                    if (kmTab[index] != 0)
                    {
                        x.Value = kmTab[index]*1000;
                    }
                    else { x.Value = null; }
                    index++;
                }
                komRng = arkuszTemp.get_Range("C20", "C39");
                index = 0;
                foreach (Excel.Range x in komRng)
                {
                    if (iriTab[index] != 0)
                    {
                        x.Value = iriTab[index];
                    }
                    else { x.Value = null; }
                    index++;
                }
                komRng = arkuszTemp.get_Range("M20", "M39");
                index = 0;
                foreach (Excel.Range x in komRng)
                {
                    if (pktTab[index] != 0)
                    {
                        x.Value = pktTab[index];
                    }
                    else { x.Value = null; }
                    index++;
                }
                komRng.Font.Color = Excel.XlRgbColor.rgbWhite;
                komRng = arkuszTemp.get_Range("N20", "N39");
                index = 0;
                foreach (Excel.Range x in komRng)
                {
                    if (iriRosTab[index] != 0)
                    {
                        x.Value = iriRosTab[index];
                    }
                    else { x.Value = null; }
                    index++;
                }
                komRng.Font.Color = Excel.XlRgbColor.rgbWhite;
                kmTab = null;
                iriTab = null;
                pktTab = null;
                iriRosTab = null;
                string kmDo = iri.wyniki[nrWynik - 1].kilometr;
                kmDo = Regex.Replace(kmDo, ",", "+");
                iri.daneProbki.kmDo.Add(kmDo);
                //kopiuje pozostałe wyniki i dane
                arkuszTemp.Cells[22, "D"].Value = rodzWarstw;
                arkuszTemp.Cells[21, "F"].Value = nrJezd;
                arkuszTemp.Cells[23, "F"].Value = pasRuch;
                arkuszTemp.Cells[25, "F"].Value = rodzPasa;
                arkuszTemp.Cells[22, "H"].Value = liczbaWskIRI;
                arkuszTemp.Cells[23, "I"].Value = iri.iri1[idxArkusza - 1];
                arkuszTemp.Cells[23, "J"].Value = iri.iri2[idxArkusza - 1];
                arkuszTemp.Cells[23, "K"].Value = iri.iri3[idxArkusza - 1];
                try
                {
                    arkuszTemp.Cells[26, "I"].Value = double.Parse(iri.iriWym1[idxArkusza - 1].Replace(".", ","));
                }
                catch { arkuszTemp.Cells[26, "I"].Value = iri.iriWym1[idxArkusza - 1]; }
                try
                {
                    arkuszTemp.Cells[26, "J"].Value = double.Parse(iri.iriWym2[idxArkusza - 1].Replace(".", ","));
                }
                catch { arkuszTemp.Cells[26, "J"].Value = iri.iriWym2[idxArkusza - 1]; }
                try
                {
                    arkuszTemp.Cells[26, "K"].Value = double.Parse(iri.iriWym3[idxArkusza - 1].Replace(".", ","));
                }
                catch { arkuszTemp.Cells[26, "K"].Value = iri.iriWym3[idxArkusza - 1]; }
            }
        } //najpierw wyniki potem dane

        public void przypiszTekstDoInfo(string x)
        {
            TextBoxInfo.Text += x;
        }
        public void UdostepnijKonwertujButton(IAsyncResult result)
        {
            konwertujButton.Dispatcher.Invoke(new UdostepnijKonwButtonDelegate(ukb), null);
        }
        public void ukb()
        {
            konwertujButton.IsEnabled = true;
        }

        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }

    }
}
