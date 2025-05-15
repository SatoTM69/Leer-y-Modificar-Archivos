using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdK_Bitch
{
    public partial class Form1 : Form
    {
        private string archivoActual = null;
        private bool cambiosPendientes = false;
        private bool cargandoArchivo = false;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Configurar ListView
            lstLista.View = View.Details;
            lstLista.FullRowSelect = true;
            lstLista.GridLines = true;
            lstLista.Columns.Add("Nombre", -2, HorizontalAlignment.Left);

            // Configurar TextBox
            txtLectura.Multiline = true;
            txtLectura.ScrollBars = ScrollBars.Both;
            txtLectura.AcceptsTab = true;
            txtLectura.ReadOnly = false;
            txtLectura.Enabled = false;

            // Configurar eventos
            txtLectura.TextChanged += txtLectura_TextChanged;
            lstLista.DoubleClick += lstLista_DoubleClick;
            lstLista.SelectedIndexChanged += lstLista_SelectedIndexChanged;
            btnAgregar.Click += btnAgregar_Click;
            btnGuardar.Click += btnGuardar_Click;

            // Desactivar botón de guardar inicialmente
            btnGuardar.Enabled = false;

            // Cargar archivos
            CargarArchivos();
        }

        private void CargarArchivos()
        {
            lstLista.Items.Clear();
            string ruta = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                foreach (var archivo in new DirectoryInfo(ruta).GetFiles("*.txt"))
                {
                    lstLista.Items.Add(archivo.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar archivos: " + ex.Message);
            }
        }

        private void lstLista_DoubleClick(object sender, EventArgs e)
        {
            if (lstLista.SelectedItems.Count == 0) return;

            if (cambiosPendientes && !string.IsNullOrEmpty(archivoActual))
            {
                var respuesta = MessageBox.Show("¿Deseas guardar los cambios antes de continuar?",
                                                "Cambios pendientes", MessageBoxButtons.YesNoCancel,
                                                MessageBoxIcon.Question);

                if (respuesta == DialogResult.Yes)
                    GuardarArchivo();
                else if (respuesta == DialogResult.Cancel)
                    return;
            }

            archivoActual = lstLista.SelectedItems[0].Text;
            CargarContenidoArchivo();
        }

        private void CargarContenidoArchivo()
        {
            cargandoArchivo = true;

            try
            {
                string rutaCompleta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, archivoActual);
                txtLectura.Text = File.ReadAllText(rutaCompleta);
                txtLectura.Enabled = true;
                txtLectura.ReadOnly = false;
                btnGuardar.Enabled = false;
                cambiosPendientes = false;
                txtLectura.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer archivo: " + ex.Message);
                txtLectura.Enabled = false;
                btnGuardar.Enabled = false;
            }
            finally
            {
                cargandoArchivo = false;
            }
        }

        private void txtLectura_TextChanged(object sender, EventArgs e)
        {
            if (!cargandoArchivo && !string.IsNullOrEmpty(archivoActual))
            {
                cambiosPendientes = true;
                btnGuardar.Enabled = true;
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese un nombre válido.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nombreArchivo = txtNombre.Text.EndsWith(".txt") ? txtNombre.Text : txtNombre.Text + ".txt";
            string rutaCompleta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo);

            if (File.Exists(rutaCompleta))
            {
                MessageBox.Show("El archivo ya existe.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                File.WriteAllText(rutaCompleta, "");
                lstLista.Items.Add(nombreArchivo);
                txtNombre.Clear();
                txtNombre.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear el archivo: " + ex.Message);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            GuardarArchivo();
        }

        private void GuardarArchivo()
        {
            if (string.IsNullOrEmpty(archivoActual))
            {
                MessageBox.Show("No hay archivo seleccionado.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string rutaCompleta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, archivoActual);
                File.WriteAllText(rutaCompleta, txtLectura.Text);
                cambiosPendientes = false;
                btnGuardar.Enabled = false;
                MessageBox.Show("Archivo guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el archivo: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cambiosPendientes)
            {
                var respuesta = MessageBox.Show("Hay cambios sin guardar. ¿Deseas guardarlos antes de salir?",
                                                "Advertencia", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (respuesta == DialogResult.Yes)
                    GuardarArchivo();
                else if (respuesta == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void lstLista_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstLista.SelectedItems.Count > 0)
                lblLectura.Text = lstLista.SelectedItems[0].Text;
            else
                lblLectura.Text = "Ningún archivo seleccionado.";
        }
    }
}