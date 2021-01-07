using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IRI_konwerter
{
    public class SRT
    {
        public SRT() { }
        private bool przeliczonoKmSRT = false;
        private string rodzajKonstrukcjiSRT, nrJezdniSRT, pasRuchuSRT, rodzajPasaSRT;
        private List<string> wynikSRT = new List<string>();
        private List<string> wymaganieSRT = new List<string>();
        private List<WynikSRT> wynikiSRT = new List<WynikSRT>();
        private DaneProbki daneProbkiSRT = new DaneProbki();

        public string rodzajKonstrukcji { get { return rodzajKonstrukcjiSRT; } set { rodzajKonstrukcjiSRT = value; } }
        public string nrJezdni { get { return nrJezdniSRT; } set { nrJezdniSRT = value; } }
        public string pasRuchu { get { return pasRuchuSRT; } set { pasRuchuSRT = value; } }
        public string rodzajPasa { get { return rodzajPasaSRT; } set { rodzajPasaSRT = value; } }
        public List<string> wynikOgolny { get { return wynikSRT; } set { wynikSRT = value; } }
        public List<string> wymaganie { get { return wymaganieSRT; } set { wymaganieSRT = value; } }
        public bool przeliczonoKm { get { return przeliczonoKmSRT; } }

        public List<WynikSRT> wyniki { get { return wynikiSRT; } set { wynikiSRT = value; } }
        public DaneProbki daneProbki { get { return daneProbkiSRT; } set { daneProbkiSRT = value; } }

        public void PrzeliczKm(string kmPoczatkowy)
        {
            if (kmPoczatkowy == null || kmPoczatkowy == "") return;
            kmPoczatkowy = Regex.Replace(kmPoczatkowy, @"\+", ",");
            double roznicaKm = double.Parse(kmPoczatkowy) - double.Parse(wynikiSRT[0].kilometr);
            foreach (WynikSRT w in wyniki)
            {
                double km = double.Parse(w.kilometr) + roznicaKm;
                w.kilometr = km.ToString();
            }
            przeliczonoKmSRT = true;
        }
    }
}
