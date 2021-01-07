using System.Collections.Generic;

namespace IRI_konwerter
{
    public class DaneProbki
    {
        public DaneProbki() { }
        private string kilometrPoczatkowyProf, dataProf, zleceniodawcaProf, nrZleceniaProf, budowaProf, drogaProf, odcinekProf, etapProf, nrProbkiProf, nrProbkiKlientProf, wymaganiaWgProf, wymaganiaProf, miejscePomiaruProf, warstwaProf, dataPomiaruProf;
        private List<string> kmOdProf = new List<string>();
        private List<string> kmDoProf = new List<string>();

        public string kilometrPoczatkowy { get { return kilometrPoczatkowyProf; } set { kilometrPoczatkowyProf = value; } }
        public string data { get { return dataProf; } set { dataProf = value; } }
        public string zleceniodawca { get { return zleceniodawcaProf; } set { zleceniodawcaProf = value; } }
        public string nrZlecenia { get { return nrZleceniaProf; } set { nrZleceniaProf = value; } }
        public string budowa { get { return budowaProf; } set { budowaProf = value; } }
        public string droga { get { return drogaProf; } set { drogaProf = value; } }
        public string odcinek { get { return odcinekProf; } set { odcinekProf = value; } }
        public string etap { get { return etapProf; } set { etapProf = value; } }
        public string nrProbki { get { return nrProbkiProf; } set { nrProbkiProf = value; } }
        public string nrProbkiKlient { get { return nrProbkiKlientProf; } set { nrProbkiKlientProf = value; } }
        public string wymagania { get { return wymaganiaProf; } set { wymaganiaProf = value; } }
        public string wymaganiaWg { get { return wymaganiaWgProf; } set { wymaganiaWgProf = value; } }
        public string miejscePomiaru { get { return miejscePomiaruProf; } set { miejscePomiaruProf = value; } }
        public string warstwa { get { return warstwaProf; } set { warstwaProf = value; } }
        public string dataPomiaru { get { return dataPomiaruProf; } set { dataPomiaruProf = value; } }
        public List<string> kmOd { get { return kmOdProf; } set { kmOdProf = value; } }
        public List<string> kmDo { get { return kmDoProf; } set { kmDoProf = value; } }
    }
}
