using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountBlock.xaml
    /// </summary>
    public partial class AccountBlock : Window
    {
        public Models.MemberATE MemberATE { get; set; }

        public AccountBlock()
        {
            InitializeComponent();
        }

        public void InitializeMember()
        {

        }
    }
}
