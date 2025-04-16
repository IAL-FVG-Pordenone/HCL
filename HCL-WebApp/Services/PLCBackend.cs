using S7.Net;

namespace progetto_AICA.Services

{
    public class PLCBackend
    {
        private static Plc _plc;
        public static bool LedCucina = false;
        public static bool LedBagno = false;
        public static bool LedSalotto = false;
        public static bool LedCamera = false;
        public PLCBackend(CpuType cpu, string ip, short rack, short slot)
        {
            _plc = new Plc(cpu, ip, rack, slot);

            cpu = CpuType.S71200;
            ip = "10.49.137.1";
            rack = 0;
            slot = 1;
        }

        public static void  Connect()
        {
            try
            {
                if (!_plc.IsConnected)
                {
                    _plc.Open();
                    Console.WriteLine("Connesso");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore connessione PLC: {ex.Message}");

            }
        }
      
        public static void Disconnect()
        {
            try
            {
                _plc?.Close();
                Console.WriteLine("Connessione al PLC chiusa");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la chiusura della connessione: {ex.Message}");
            }
        }
        //Esempio lettura dati
        public static void DataRead()
        {
            try
            {
                //public object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount)
                var data = _plc.Read(DataType.DataBlock, 1, 0, VarType.Int, 1);
                //byte[] data = (byte[])_plc.Read(""); //Inserire il valore da leggere
                Console.WriteLine("Dati letti: " + data.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la lettura dei dati: {ex.Message}");
            }
        }
        public static void lampadaCucina()
        {
            try
            {
                if (LedCucina == false)
                {
                    _plc.Write("DB1.DBX0.1", true);
                    Console.WriteLine("Lampadina Q0.1 Accesa");
                    LedCucina = true;
                }
                else
                {
                    LedCucina = false;
                    _plc.Write("DB1.DBX0.1", false);
                    Console.WriteLine("Lampadina Q0.1 Spenta");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore:{ex.Message}");
            }
        }
        public static void lampadaSalotto()
        {
            try
            {
                if (LedSalotto == false)
                {
                    LedSalotto = true;
                    _plc.Write("DB1.DBX0.5", true);
                    Console.WriteLine("Lampadina Q0.5 Accesa");
                }
                else
                {
                    LedSalotto = false;
                    _plc.Write("DB1.DBX0.5", false);
                    Console.WriteLine("Lampadina Q0.5 Spento");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore:{ex.Message}");
            }
        }
        public static void lampadaBagno()
        {
            try
            {
                if (LedBagno == false)
                {
                    _plc.Write("DB1.DBX0.2", true);
                    Console.WriteLine("Lampadina Q0.2 Accesa");
                    LedBagno = true;
                }
                else
                {
                    LedBagno = false;
                    _plc.Write("DB1.DBX0.2", false);
                    Console.WriteLine("Lampadina Q0.2 Spento");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore:{ex.Message}");
            }
        }
        public static void lampadaCamera()
        {
            try
            {
                if (LedCamera == false)
                {
                    LedCamera = true;
                    _plc.Write("DB1.DBX0.6", true);
                    Console.WriteLine("Lampadina Q0.6 Accesa");
                }
                else
                {
                    LedCamera = false;
                    _plc.Write("DB1.DBX0.6", false);
                    Console.WriteLine("Lampadina Q0.6 Spento");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore:{ex.Message}");
            }
        }
    }
}
