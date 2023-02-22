using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Media;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace nom
{
    /*
     
     attention les BTM en 1, 4 et 8 bit n'ayant pas assez d'exemple sur internet, j'ai fais une petit manip en esperant que ca fonctionne 
    (il y a des des cases dans imges pour lesquelles je n'ai pas trouve d'explications
    les BTM en 32 bits sont inaccessible par mon ordinateur donc ils ne sont pas pris en compte
     */
    public class Program
    {
        static void Main(string[] args)
        {

            bool jouer = true;   
            while (jouer)
            {
                Console.Clear();
                Console.WriteLine("entrer un fichier à lire :");
                MyImage nom_fichier = new MyImage(Convert.ToString(Console.ReadLine()));
                Console.Clear();
                Console.WriteLine("Bonjour, vous allez pouvoir choisir le numero d'exercice que vous souhaitez effectuer");
                Console.WriteLine("1: Image en noir et blanc\n" 
                    + "2: Image en nuance de gris\n" + "3: Agrandir l'image\n" + "4: Retrecir l'image\n"
                    + "50: Rotation de l'image conservation des pixels\n51: Rotation de l'image conservation des distances" + "6: Miroir central de l'image\n" + "7: Application d'un filtre sur l'image\n" 
                    + "8: Fractale de Mandelbrot\n" + "9: Histogramme de l'image\n" + "10: Coder/Encoder une image+\n" 
                    + "11: Generer un QRCode\n 12: Lire un QRcode");
                int reponse = Convert.ToInt32(Console.ReadLine());
                List<string> nom_fichiers = new List<string>();
                switch (reponse)
                {
                    case 1:
                        nom_fichier.Noir_et_blanc();
                        nom_fichier.From_Image_To_File("nom_fichier_Noir_et_Blanc");
                        nom_fichiers.Add("nom_fichier_Noir_et_Blanc.bmp");
                        break;
                    case 2:
                        nom_fichier.Nuances_de_gris();
                        nom_fichier.From_Image_To_File("nom_fichier_En_Gris");
                        nom_fichiers.Add("nom_fichier_En_Gris.bmp");
                        break;
                    case 3:
                        Console.WriteLine("Veuillez renseignez le coefficient d'aggrandissement");
                        int answer = Convert.ToInt32(Console.ReadLine());
                        nom_fichier.Agrandir(answer);
                        nom_fichiers.Add("nom_fichier_Aggrandit.bmp");
                        break;
                    case 4:
                        Console.WriteLine("Veuillez renseignez le coefficient de retrecissement");
                        int rep = Convert.ToInt32(Console.ReadLine());
                        nom_fichier.Retrecir(rep);
                        nom_fichiers.Add("nom_fichier_Retrecit.bmp");
                        break;
                    case 50:
                        Console.WriteLine("Veuillez renseignez le degre de rotation");
                        int degre_pixel = Convert.ToInt32(Console.ReadLine());
                        nom_fichier.Rotation(nom_fichier.Image, degre_pixel);
                        ///nom_fichier.From_Image_To_File("nom_fichier_Rotate");
                        nom_fichiers.Add("rotation_pixel"+ degre_pixel + ".bmp");
                        /// Ecrit le truc pour la rotation pck jsp quelles fonctions il faut appeler
                        break;
                    case 51:
                        Console.WriteLine("Veuillez renseignez le degre de rotation");
                        int degre_distance = Convert.ToInt32(Console.ReadLine());
                        nom_fichier.RotationV2(degre_distance);
                        ///nom_fichier.From_Image_To_File("nom_fichier_Rotate");
                        nom_fichiers.Add("rotation_distance" + degre_distance + ".bmp");
                        /// Ecrit le truc pour la rotation pck jsp quelles fonctions il faut appeler
                        break;
                    case 6:
                        nom_fichier.Effet_Miroir();
                        //Process.Start("nom_fichier_Miroir.bmp");
                        break;
                    case 7:
                        Console.WriteLine("Veuillez choisir un nombre correspondant au filtre que vous voulez appliquer\n");
                        Console.WriteLine("1: Floutage\n" + "2: Renforcement des bords\n" + "3: Detection des bords\n" + "4: Repoussage des bords");
                        int numero = Convert.ToInt32(Console.ReadLine());
                        switch (numero)
                        {
                            case 1:
                                int[,] matrice_floutage = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
                                nom_fichier.convolution(nom_fichier.Image, matrice_floutage);
                                ///nom_fichier.From_Image_To_File("nom_fichier_Floutage");
                                nom_fichiers.Add("convolution.bmp");
                                break;
                            case 2:
                                int[,] matrice_bords = { { 0, 0, 0 }, { -27, 27, 0 }, { 0, 0, 0 } };
                                nom_fichier.convolution(nom_fichier.Image, matrice_bords);
                                ///nom_fichier.From_Image_To_File("nom_fichier_Renforcement");
                                nom_fichiers.Add("convolution.bmp");
                                break;
                            case 3:
                                int[,] matrice_bords_dectect = { { 0, 8, 0 }, { 8, -32, 8 }, { 0, 8, 0 } };
                                nom_fichier.convolution(nom_fichier.Image, matrice_bords_dectect);
                                ///nom_fichier.From_Image_To_File("nom_fichier_Detection");
                                nom_fichiers.Add("convolution.bmp");
                                break;
                            case 4:
                                int[,] matrice_repoussage = { { -18, -9, 0 }, { -9, 9, 9 }, { 0, 9, 18 } };
                                nom_fichier.convolution(nom_fichier.Image, matrice_repoussage);
                                ///nom_fichier.From_Image_To_File("nom_fichier_Repoussage");
                                nom_fichiers.Add("convolution.bmp");
                                break;
                            default:
                                Console.WriteLine("Veuillez renseignez un numero valable");
                                break;
                        }
                        break;
                    case 8:
                        Console.WriteLine("Veuillez renseignez le nombre d'itérations de la fractale");
                        int iteration = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Veuillez renseignez le coefficient de bleu que vous souhaitez");
                        int bleu = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Veuillez renseignez le coefficient de vert que vous souhaitez");
                        int vert = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Veuillez renseignez le coefficient de rouge que vous souhaitez");
                        int rouge = Convert.ToInt32(Console.ReadLine());
                        nom_fichier.Fractale_de_Mandelbrot(iteration, bleu, vert, rouge);
                        ///nom_fichier.From_Image_To_File("Mandelbrot");
                        nom_fichiers.Add("Mandelbrot.bmp");
                        break;
                    case 9:
                        nom_fichier.histogramme(nom_fichier.Image);
                        nom_fichiers.Add("histogramme_affichageacote.bmp");
                        break;

                    case 10:
                        MyImage support_image_cacher = new MyImage("lena");
                        Console.WriteLine("Nous allons d'abors cacher nom_fichier dans une image ");
                        nom_fichier.cacher_une_image(nom_fichier.Image, support_image_cacher.Image);
                        nom_fichiers.Add("cacher.bmp");
                        Console.WriteLine("Voulez vous dévoiler l'image qui est cachée dans nom_fichier?");
                        Console.WriteLine("Si oui taper 1 sinon tapez 2");
                        int reponses = Convert.ToInt32(Console.ReadLine());
                        switch (reponses)
                        {
                            case 1:
                                support_image_cacher.devoiler_une_image(support_image_cacher.Image);
                                nom_fichiers.Add("devoilage 1.bmp");
                                nom_fichiers.Add("devoilage 2.bmp");
                                break;

                            case 2:
                                break;
                        }
                        break;
                    case 11:
                        Console.Clear();
                        Console.WriteLine("entrer un phrase à encoder :");
                        string phrase = Convert.ToString(Console.ReadLine());
                        QR_code qr_code_encodage = new QR_code(phrase);
                        Pixel[,] image_final = qr_code_encodage.ecriture_QR_code(qr_code_encodage.construction_message_QR_code());
                        MyImage qr_code_encodage_tableau = new MyImage(image_final);
                        Console.WriteLine("fichier bmp disponible dans : ");
                        qr_code_encodage_tableau.From_Image_To_File(qr_code_encodage_tableau.Effet_Miroir_bas_vers_haut());
                        break;
                    case 12:

                        MyImage qr_code_decodage_image = new MyImage(nom_fichier.Image);
                        QR_code qr_code_decodage = new QR_code(qr_code_decodage_image.Image);
                        Console.WriteLine(qr_code_decodage.Mot);
                        break;
                    default:
                        Console.WriteLine("Veuillez entrez un numéro d'éxercice valable");
                        break;

                }
                for(int i =0; i < nom_fichiers.Count; i++){
                    Process.Start(new ProcessStartInfo(System.IO.Directory.GetCurrentDirectory() + "/"+nom_fichiers[i]) { UseShellExecute = true });
                }
                
                
                Console.WriteLine("arrêter de jouer (1)");
                string a = Convert.ToString(Console.ReadKey());


                if(a != "1")
                {
                    jouer = false;
                }
            }
        }
    }
}