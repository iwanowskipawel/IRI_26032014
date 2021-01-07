namespace IRI_konwerter
{
    public class WynikIRI
    {
        public WynikIRI() { }

        private string kilometrProf, iriProf, punktProf, iriRosnaceProf;

        public string kilometr { get { return kilometrProf; } set { kilometrProf = value; } }
        public string iri { get { return iriProf; } set { iriProf = value; } }
        public string punkt { get { return punktProf; } set { punktProf = value; } }
        public string iriRosnace { get { return iriRosnaceProf; } set { iriRosnaceProf = value; } }
    }
}
