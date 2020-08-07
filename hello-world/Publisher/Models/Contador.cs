namespace Publisher
{
    public class Contador
    {
        private int _valor = 0;

        public void Incrementar()
        {
            this._valor++;
        }

        public int ValorAtual => _valor;
    }
}
