
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NamoDoApp
{
    public class  MemoriaAvancada : IDisposable
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        private IntPtr handleProcesso;
        private string nomeProcesso;

        public MemoriaAvancada(string nomeProcesso)
        {
            this.nomeProcesso = nomeProcesso;
            Inicializar();
        }

        private void Inicializar()
        {
            try
            {
                var processos = Process.GetProcessesByName(nomeProcesso);
                if (processos.Length == 0)
                    throw new Exception($"Processo '{nomeProcesso}' não encontrado");

                var processo = processos[0];
                handleProcesso = OpenProcess(PROCESS_ALL_ACCESS, false, processo.Id);

                if (handleProcesso == IntPtr.Zero)
                    throw new Exception("Erro pra abrir");

                Console.WriteLine($"Processo {nomeProcesso} abriu de boa. Handle: {handleProcesso}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro p iniciar: {ex.Message}");
                throw;
            }
        }

        public IntPtr ObterEnderecoBaseModulo(string nomeModulo)
        {
            try
            {
                var processos = Process.GetProcessesByName(nomeProcesso);
                if (processos.Length == 0)
                {
                    Console.WriteLine("Processo não encontrado ao buscar módulo");
                    return IntPtr.Zero;
                }

                var processo = processos[0];

                foreach (ProcessModule modulo in processo.Modules)
                {
                    if (modulo.ModuleName.Equals(nomeModulo, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Módulo {nomeModulo} encontrado: 0x{modulo.BaseAddress.ToInt64():X}");
                        return modulo.BaseAddress;
                    }
                }

                Console.WriteLine($"Módulo {nomeModulo} não encontrado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter módulo: {ex.Message}");
            }

            return IntPtr.Zero;
        }

        public bool EscreverBytes(IntPtr endereco, byte[] bytes)
        {
            try
            {
                bool sucesso = WriteProcessMemory(handleProcesso, endereco, bytes, bytes.Length, out int bytesEscritos);

                if (!sucesso || bytesEscritos != bytes.Length)
                {
                    Console.WriteLine($"Falha ao escrever memória. Sucesso: {sucesso}, Bytes escritos: {bytesEscritos}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao escrever bytes: {ex.Message}");
                return false;
            }
        }

        public bool EscreverNops(IntPtr endereco, int quantidade)
        {
            byte[] nops = new byte[quantidade];
            for (int i = 0; i < quantidade; i++)
                nops[i] = 0x90;

            return EscreverBytes(endereco, nops);
        }


        public unsafe T Ler<T>(IntPtr endereco) where T : unmanaged
        {
            int tamanho = sizeof(T);
            byte[] buffer = new byte[tamanho];

            bool sucesso = ReadProcessMemory(handleProcesso, endereco, buffer, tamanho, out int bytesLidos);

            if (!sucesso || bytesLidos != tamanho)
            {
                Console.WriteLine($"Falha ao ler no endereço 0x{endereco.ToInt64():X}");
                return default;
            }

            fixed (byte* p = buffer)
            {
                return *(T*)p;
            }
        }

        public unsafe bool Escrever<T>(IntPtr endereco, T valor) where T : unmanaged
        {
            int tamanho = sizeof(T);
            byte[] buffer = new byte[tamanho];

            fixed (byte* p = buffer)
            {
                *(T*)p = valor;
            }

            bool sucesso = WriteProcessMemory(handleProcesso, endereco, buffer, tamanho, out int bytesEscritos);

            if (!sucesso || bytesEscritos != tamanho)
            {
                Console.WriteLine($"Falha ao escrever no endereço 0x{endereco.ToInt64():X}");
                return false;
            }

            return true;
        }

        
        public void Fechar()
        {
            if (handleProcesso != IntPtr.Zero)
            {
                CloseHandle(handleProcesso);
                handleProcesso = IntPtr.Zero;
                Console.WriteLine("Handle do processo fechado");
            }
        }

        public void Dispose()
        {
            Fechar();
            GC.SuppressFinalize(this);
        }

        MemoriaAvancada()
        {
            Fechar();
        }
    }
}
