using ClickableTransparentOverlay;
using ImGuiNET;
using NamoDoApp;
using System;
using System.Threading;

namespace GITCS2
{
    public class Program : Overlay
    {
        bool espAtivo = false;
        bool fecharESP = true;
        MemoriaAvancada memoriaAvancada;

        // --- Cheating Logic ---
        public void cheatinglogic()
        {
            int posESP = 0xBDAD7C; //sempre pegar novo "valor hex" a cada atualizacao do CS.
            memoriaAvancada = new MemoriaAvancada("cs2");
            IntPtr playerEndereco = memoriaAvancada.ObterEnderecoBaseModulo("client.dll");
            IntPtr ESPendereco = playerEndereco + posESP;

            while (true)
            {
                if (espAtivo)
                {
                    // Ativa o ESP
                    memoriaAvancada.EscreverNops(ESPendereco, 2);
                }
                else
                {
                    // Desativa o ESP (restaura os bytes originais)
                    memoriaAvancada.EscreverBytes(ESPendereco, new byte[] { 0x32, 0xC0 });
                }

                //thread pra nao travar tanto o jogo
                Thread.Sleep(100);
            }
        }
       

        //Olho que eu pedi pra IA fazer. Apenas a fins de estilização.
        private void DesenharOlhoLaranja()
        {
            var drawList = ImGui.GetWindowDrawList();
            var pos = ImGui.GetCursorScreenPos();

            uint corLaranja = ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 0.4f, 0.0f, 1.0f));

            float cx = pos.X + 12;
            float cy = pos.Y + 10;

            drawList.AddBezierCubic(
                new System.Numerics.Vector2(cx - 8, cy),
                new System.Numerics.Vector2(cx - 3, cy - 7),
                new System.Numerics.Vector2(cx + 3, cy - 7),
                new System.Numerics.Vector2(cx + 8, cy),
                corLaranja, 1.5f);

            drawList.AddBezierCubic(
                new System.Numerics.Vector2(cx - 8, cy),
                new System.Numerics.Vector2(cx - 3, cy + 7),
                new System.Numerics.Vector2(cx + 3, cy + 7),
                new System.Numerics.Vector2(cx + 8, cy),
                corLaranja, 1.5f);

            drawList.AddCircleFilled(new System.Numerics.Vector2(cx, cy), 2.5f, corLaranja);

            ImGui.Dummy(new System.Numerics.Vector2(25, 16));
            ImGui.SameLine();
        }

        protected override void Render()
        {
            if (!fecharESP)
            {
                Close();
            }

            var style = ImGui.GetStyle();
            style.WindowRounding = 8.0f;
            style.FrameRounding = 4.0f;
            style.PopupRounding = 4.0f;
            style.WindowTitleAlign = new System.Numerics.Vector2(0.5f, 0.5f); 

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0.08f, 0.08f, 0.08f, 0.95f)); 
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new System.Numerics.Vector4(0.5f, 0.1f, 0.1f, 1.0f)); 

            ImGui.PushStyleColor(ImGuiCol.CheckMark, new System.Numerics.Vector4(1.0f, 0.4f, 0.0f, 1.0f)); 
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1.0f)); 
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new System.Numerics.Vector4(0.5f, 0.2f, 0.0f, 1.0f)); 

            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f)); 
            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.5f, 0.1f, 0.1f, 1.0f)); 
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.7f, 0.2f, 0.2f, 1.0f));

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 90));

            // menu
            ImGui.Begin("LOGAAAAAA SEU GORILA.(FINISH,2020).", ref fecharESP, ImGuiWindowFlags.NoResize);

            ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.6f, 0.0f, 1.0f), "VOCÊ LOGOU TEM 3 DIAS.");
            ImGui.Separator(); 

            string iconeOlho = "\uf06e "; 

            DesenharOlhoLaranja();

            ImGui.Checkbox("LOGIN DO DEMON", ref espAtivo);

            ImGui.End();

            
            ImGui.PopStyleColor(9);
        }

        // main
        static void Main(string[] args)
        {
            Console.WriteLine("Essa mensagem será carregada no console pra você lembrar o quão bagre você é.");

            Program programa = new Program();

            Thread hackThread = new Thread(programa.cheatinglogic) { IsBackground = true };
            hackThread.Start();

            programa.Start().Wait();
        }
    }
}