using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IRI_konwerter
{
    public class IRI
    {
        public IRI() { }
        private bool przeliczonoKmIRI = false;
        private string rodzajKonstrukcjiIRI, nrJezdniIRI, pasRuchuIRI, rodzajPasaIRI;
        private List<string> iri1IRI = new List<string>();
        private List<string> iri2IRI = new List<string>();
        private List<string> iri3IRI = new List<string>();
        private List<string> iriWym1IRI = new List<string>();
        private List<string> iriWym2IRI = new List<string>();
        private List<string> iriWym3IRI = new List<string>();
        private List<WynikIRI> wynikiIRI = new List<WynikIRI>();
        private DaneProbki daneProbkiIRI = new DaneProbki();

        public string rodzajKonstrukcji { get { return rodzajKonstrukcjiIRI; } set { rodzajKonstrukcjiIRI = value; } }
        public string nrJezdni { get { return nrJezdniIRI; } set { nrJezdniIRI = value; } }
        public string pasRuchu { get { return pasRuchuIRI; } set { pasRuchuIRI = value; } }
        public string rodzajPasa { get { return rodzajPasaIRI; } set { rodzajPasaIRI = value; } }
        public List<string> iri1 { get { return iri1IRI; } set { iri1IRI = value; } }
        public List<string> iri2 { get { return iri2IRI; } set { iri2IRI = value; } }
        public List<string> iri3 { get { return iri3IRI; } set { iri3IRI = value; } }
        public List<string> iriWym1 { get { return iriWym1IRI; } set { iriWym1IRI = value; } }
        public List<string> iriWym2 { get { return iriWym2IRI; } set { iriWym2IRI = value; } }
        public List<string> iriWym3 { get { return iriWym3IRI; } set { iriWym3IRI = value; } }
        public bool przeliczonoKm { get { return przeliczonoKmIRI; } }

        public List<WynikIRI> wyniki { get { return wynikiIRI; } set { wynikiIRI = value; } }
        public DaneProbki daneProbki { get { return daneProbkiIRI; } set { daneProbkiIRI = value; } }

        public void PrzeliczKm(string kmPoczatkowy)
        {
            if (kmPoczatkowy == null || kmPoczatkowy == "") return;
            kmPoczatkowy = Regex.Replace(kmPoczatkowy, @"\+", ",");
            double roznicaKm = double.Parse(kmPoczatkowy) - double.Parse(wynikiIRI[0].kilometr);
            foreach (WynikIRI w in wyniki)
            {
                double km = double.Parse(w.kilometr) + roznicaKm;
                w.kilometr = km.ToString();
            }
            przeliczonoKmIRI = true;
        }
    }
}
