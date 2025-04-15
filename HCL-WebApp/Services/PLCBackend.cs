using S7.Net;

namespace progetto_AICA.Services

{
    public class PLCBackend
    {
        private static Plc _plc;
        public static int LedCucina = 0;
        public static int LedBagno = 0;
        public static int LedSalotto = 0;
        public static int LedCamera = 0;
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
        public static void dataWrite()
        {
            Console.WriteLine("Inserisci il numero della lampadina: (1, 2, 3, 4)");
            try
            {
                while (true)
                {
                    string input = Console.ReadLine();
                    switch (input)
                    {
                        case "1":
                            _plc.Write("Q0.0", true);
                            Console.WriteLine("Lampadina Q0.0 Accesa");
                            break;
                        case "2":
                            _plc.Write("Q0.0", false);
                            Console.WriteLine("Lampadina Q.0.0 SPENTA");
                            break;
                        case "3":

                            break;
                        default:
                            Console.WriteLine("Comando non riconosciuto");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore:{ex.Message}");
            }
            finally
            {
                /*if (_plc.IsConnected)
{
    _plc.Close();
    Console.WriteLine("Connessione al PLC chiusa.");
}*/
            }
        }
        public static void lampadaCucina()
        {
            try
            {
                if (LedCucina == 0)
                {
                    _plc.Write("DB1.DBX0.1", true);
                    Console.WriteLine("Lampadina Q0.1 Accesa");
                    LedCucina = 1;
                }
                else
                {
                    LedCucina = 0;
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
                if (LedSalotto == 0)
                {
                    LedSalotto = 1;
                    _plc.Write("DB1.DBX0.5", true);
                    Console.WriteLine("Lampadina Q0.5 Accesa");
                }
                else
                {
                    LedSalotto = 0;
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
                if (LedBagno == 0)
                {
                    _plc.Write("DB1.DBX0.2", true);
                    Console.WriteLine("Lampadina Q0.2 Accesa");
                    LedBagno = 1;
                }
                else
                {
                    LedBagno = 0;
                    _plc.Write("DB1.DBX0.2", false);
                    Console.WriteLine("Lampadina Q0.2 Spento");
                }
            }
            //malcom
            catch (Exception ex)
            {
                Console.WriteLine($"Errore:{ex.Message}");
            }
            finally
            {
                /*if (_plc.IsConnected)
 {
     _plc.Close();
     Console.WriteLine("Connessione al PLC chiusa.");
 }*/
            }
        }
        public static void lampadaCamera()
        {
            try
            {
                if (LedCamera == 0)
                {
                    LedCamera = 1;
                    _plc.Write("DB1.DBX0.6", true);
                    Console.WriteLine("Lampadina Q0.6 Accesa");
                }
                else
                {
                    LedCamera = 0;
                    _plc.Write("DB1.DBX0.6", false);
                    Console.WriteLine("Lampadina Q0.6 Spento");
                }
            }
            //malcom
            catch (Exception ex)
            {
                Console.WriteLine($"Errore:{ex.Message}");
            }
            finally
            {
                /*if (_plc.IsConnected)
 {
     _plc.Close();
     Console.WriteLine("Connessione al PLC chiusa.");
 }*/
            }
        }
    }
}
