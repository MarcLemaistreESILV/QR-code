using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;



namespace nom
{

        public class MyImage
        {


            /// <summary>
            /// only works for image that are after windows 2000 a depth of 24 (so most commom) and a compression format 0 (well known rectangular image)
            /// in other words only works for the typical rectangle image in an average pc -> when the image seems advanced => won't work
            /// </summary>

            private int taille;
            private int offset;
            private int largeur;
            private int hauteur;
            private int bpp;
            private Pixel[,] image;

            private int constante_de_bit;

            public byte[] images;

            public MyImage(string nom, bool Qr_code)
            {
                this.taille = 0;
                this.offset = 54;
                this.largeur = 0;
                this.hauteur = 0;
                this.bpp = 24;
                this.image = null;
            }
            public MyImage(Pixel[,] image)
            {
                this.taille = image.Length;
                this.offset = 54;
                this.largeur = image.GetLength(1);
                this.hauteur = image.GetLength(0);
                this.bpp = 24;
                this.image = image;
           
            }
            public MyImage(string myfile)
            {


                byte[] images = File.ReadAllBytes("./" + myfile + ".bmp");
                this.images = images;

                string type = Convert.ToChar(images[0]) + " " + Convert.ToChar(images[1]);
                //taille
                int taille = 0;
                int puissance = 1;
                for (int i = 2; i < 6; i++)
                {
                    taille += images[i] * puissance;
                    puissance = puissance * 256;
                }
                //offset
                int offset = 0;
                puissance = 1;
                for (int i = 10; i < 14; i++)
                {
                    offset += images[i] * puissance;
                    puissance = puissance * 256;
                }
                //width
                int width = 0;
                puissance = 1;
                for (int i = 18; i < 22; i++)
                {

                    width += images[i] * puissance;
                    puissance = puissance * 256;
                }
                //height
                int height = 0;
                puissance = 1;
                for (int i = 22; i < 26; i++)
                {

                    height += images[i] * puissance;
                    puissance = puissance * 256;
                }

                //bpp
                int bpp = 0;
                puissance = 1;
                for (int i = 28; i < 30; i++)
                {

                    bpp += images[i] * puissance;
                    puissance = puissance * 256;
                }
                //constante de bit
                int constante_de_bit = 0;
                if (bpp != 24)//représente la place nécessaire pour stocker la conversion de couleur de n bits vers 24 bits (1 oct/couleur)
                {
                    constante_de_bit = (int)(Math.Pow(2, bpp + 2));
                }
                this.constante_de_bit = constante_de_bit;
                this.bpp = bpp;
                this.offset = offset;
                this.taille = taille;
                this.largeur = width;
                this.hauteur = height;
                this.image = new Pixel[height, width];

                image = creation_de_image(images, largeur, hauteur);
            }

            public Pixel[,] creation_de_image(byte[] tableau, int largeur, int hauteur)//fonctionne que pour des multiples de 8
            {
                if (bpp >= 8 && bpp < 25)//ici on est pas en RGB  l'oeil ayant plus d'affinité pour le vert (cf spectre des cones) on décide de rajouter des couleurs vertes si pas divisible par trois -> ont permet le passage en RGB
                {
                    //mets les couleurs de bpp/3 dans une liste
                    List<List<int>> result_transf = new List<List<int>>();

                    //i * ((ligne_bits.Count/8 ) - (((ligne_bits.Count+3) / 8)%4)) + (k/8) + offset
                    //on converti tout en bit qu'on met dans la liste
                    int taille_ligne = (largeur * bpp / 8) + ((4 - (largeur * bpp / 8) % 4) % 4);
                    for (int i = 54 + constante_de_bit; i <= tableau.Length - taille_ligne; i += taille_ligne)//me deuxième membres correspond à l'octet à moitié complété
                    {
                        List<int> result_ligne = new List<int>();
                        for (int j = 0; j < taille_ligne; j++)//ajouter les bits de l'octet suivant mais par rempli (l'arrondit)
                        {
                            int[] bit_dans_octet = Convertir_int_en_binaire(tableau[i + j]); //
                            for (int k = 0; k < 8; k++)
                            {
                                result_ligne.Add(bit_dans_octet[k]);
                            }
                        }
                        result_transf.Add(result_ligne);
                    }
                    //on a donc une liste avec tout les bits il nous reste plus qu'à convertir ces bits en couleur selon bpp
                    for (int i = 0; i < hauteur; i++)
                    {
                        for (int j = 0; j < largeur; j++)
                        {
                            //attention c'est blue, green, red pour les couleurs
                            //ici on converti les bits en couleurs
                            int puissance = 1;
                            int maxb = 0;
                            int bleu = 0;
                            int avancement = 0;//pas nécessaire mais simplifie la lecture
                            for (int k = 0; k < (((bpp + 2) / 3) - (((bpp + 2) / 3) % 1)); k++)
                            {
                                bleu += result_transf[i][(j * bpp) + avancement] * puissance;
                                avancement++;
                                maxb += puissance;
                                puissance = puissance * 2;

                            }

                            int vert = 0;
                            int maxv = 0;
                            puissance = 1;
                            for (int k = 0; k < (((bpp - bpp % 3) / 3) + ((bpp % 3) / 2)); k++)
                            {
                                vert += result_transf[i][(j * bpp) + avancement] * puissance;
                                maxv += puissance;
                                puissance = puissance * 2;
                                avancement++;

                            }

                            int rouge = 0;
                            int maxr = 0;
                            puissance = 1;
                            for (int k = 0; k < ((bpp - bpp % 3) / 3); k++)
                            {
                                rouge += result_transf[i][j * bpp + avancement] * puissance;
                                maxr += puissance;
                                puissance = puissance * 2;
                                avancement++;
                            }
                            //on les passe en codage sur 8 octet/couleur
                            rouge = 255 * rouge / maxr;//règle de 3
                            vert = 255 * vert / maxv;
                            bleu = 255 * bleu / maxb;
                            //on peut les entrer dans image
                            image[i, j] = new Pixel((byte)(bleu), (byte)(vert), (byte)(rouge));
                            // Console.WriteLine(image[i, j].ToString());
                        }

                    }

                }
                else if (bpp >= 3)
                {
                    //mets les couleurs de bpp/3 dans une liste
                    List<List<int>> result_transf = new List<List<int>>();
                    //on converti tout en bit qu'on mets dans la liste
                    int taille_une_ligne_oct = ((largeur * bpp / 8) + 4 - (largeur * bpp / 8) % 4);
                    for (int i = 54 + constante_de_bit; i < 54 + constante_de_bit + hauteur * taille_une_ligne_oct; i += taille_une_ligne_oct)
                    {
                        List<int> result_ligne = new List<int>();
                        for (int j = 0; j < ((bpp * largeur + 8 - (bpp * largeur) % 8) / 8); j++)
                        {
                            int[] bit_dans_octet = Convertir_int_en_binaire(images[i + j]);
                            for (int k = 0; k < 8; k++)
                            {
                                result_ligne.Add(bit_dans_octet[7 - k]);//car enregistré en little indiane
                            }
                        }
                        result_transf.Add(result_ligne);
                    }
                    //on a donc une liste avec tout les bits il nous reste plus qu'à convertir ces bits en couleur selon bpp
                    for (int i = 0; i < hauteur; i++)
                    {
                        for (int j = 0; j < largeur; j++)
                        {
                            //ici on converti les bits en couleurs
                            int puissance = 1;
                            int rouge = 0;
                            for (int k = 0; k < (bpp - bpp % 3) / 3; k++)
                            {
                                rouge += result_transf[i][j * bpp];
                                puissance = puissance * 2;
                            }
                            int vert = 0;
                            puissance = 1;
                            for (int k = 0; k < (((bpp - bpp % 3) / 3) + ((bpp % 3) / 2)); k++)
                            {
                                vert += result_transf[i][j * bpp];
                                puissance = puissance * 2;
                            }
                            int bleu = 0;
                            puissance = 1;
                            for (int k = 0; k < (bpp + bpp % 3) / 3; k++)
                            {
                                bleu += result_transf[i][j * bpp];
                                puissance = puissance * 2;
                            }
                            //on les passe en codage sur 8 octet/couleur
                            rouge = 255 * rouge / ((bpp - bpp % 3) / 3);
                            vert = 255 * vert / ((bpp - 1) / 3);
                            bleu = 255 * bleu / ((bpp + bpp % 3) / 3);
                            //on peut les entrer dans image
                            image[i, j] = new Pixel((byte)(rouge), (byte)(vert), (byte)(bleu));
                        }
                    }
                }
                else
                {
                    //mets les couleurs de bpp/3 dans une liste
                    List<List<int>> result_transf = new List<List<int>>();
                    //on converti tout en bit qu'on mets dans la liste
                    for (int i = 54 + constante_de_bit; i < images.Length; i += bpp)
                    {
                        List<int> result_ligne = new List<int>();
                        for (int j = 0; j < bpp; j++)
                        {
                            int[] bit_dans_octet = Convertir_int_en_binaire(images[i + j]); //
                            for (int k = 0; k < 8; k++)
                            {
                                result_ligne.Add(bit_dans_octet[k]);
                            }
                        }
                        result_transf.Add(result_ligne);
                    }
                    //on a donc une liste avec tout les bits il nous reste plus qu'à convertir ces bits en couleur selon bpp
                    for (int i = 0; i < hauteur; i++)
                    {
                        for (int j = 0; j < largeur; j++)
                        {
                            //ici on converti les bits en couleurs
                            if (result_transf[i][j * bpp] == 0)
                            {
                                image[i, j] = new Pixel((byte)(0), (byte)(0), (byte)(0));
                            }
                            else
                            {
                                image[i, j] = new Pixel((byte)(255), (byte)(255), (byte)(255));
                            }
                            //on peut les entrer dans image

                        }
                    }
                }
                return image;
            }
            public (int, int) Trouver_couleur_byte(int[] octet, int position, int bpp, int color)
            {
                int result = 0;
                int comptage = 0;
                if (color == 0)
                {
                    comptage = (bpp - bpp % 3) / 3;
                }
                else if (color == 1)
                {
                    comptage = (bpp - 1) / 3;
                }
                else
                {
                    comptage = bpp / 3;
                }

                int puissance = 1;
                for (int k = 0; k < comptage; k++)
                {
                    result += octet[position + k] * puissance;
                    puissance = puissance * 2;
                }
                return (result, comptage + 1);   //resut la couleur et comaptage+1 nombre de bit utilisé         
            }
            public byte Convertir_bit_en_int(int[] tab_bit)
            {
                int puissance = 1;
                byte somme = 0;
                for (int i = 0; i < tab_bit.Length; i++)
                {
                    somme += (byte)(tab_bit[i] * puissance);
                    puissance = puissance * 2;
                }
                return somme;
            }

            public int Hauteur
            {
                get { return hauteur; }
                set { hauteur = value; }
            }

            public int Largeur
            {
                get { return largeur; }
                set { largeur = value; }
            }
            public Pixel[,] Image
            {
                get { return image; }
                set { image = value; }
            }
            public int Taille
            {
                get { return taille; }

                set { taille = value; }
            }
            public int Offset
            {
                get { return offset; }
                set { offset = value; }
            }
            public int Bpp
            {
                get { return bpp; }
                set { bpp = value; }
            }
            #region from image to file
           
         public void From_Image_To_File(string file)
            {
                int taille_ligne_oct = (largeur * bpp / 8) + 4 - ((largeur * bpp / 8) % 4);
                int offset = 54;
                if (bpp != 24)
                {
                    offset += (int)(Math.Pow(2, bpp + 2));
                }
                byte[] taille_du_offset = Convertir_Int_To_Endian(offset);//ou constante_de_bit + 54
                byte[] largeur_de_image = Convertir_Int_To_Endian(largeur);
                byte[] hauteur_de_image = Convertir_Int_To_Endian(hauteur);
                byte[] taille_du_fichier = Convertir_Int_To_Endian(offset + hauteur * taille_ligne_oct);//c'est faux (sauf pour les multpile de 8 != 1) mais ca veut dire le nombre d'octet qu'utilise une couleur
                byte[] sizeofimageConverted = Convertir_Int_To_Endian(hauteur * taille_ligne_oct);
                byte[] taille_image = Convertir_Int_To_Endian(image.Length);
                byte[] tableau = new byte[offset + hauteur * taille_ligne_oct];



                tableau[0] = 66; // Car 42 en héxadécimal donne 66 en décimal
                tableau[1] = 77; // Car 4D en héxadécimal donne 77 en décimal

                for (int i = 2; i < 6; i++)
                {
                    tableau[i] = taille_du_fichier[i - 2]; // De l'octet 2 à 6, taille du fichier
                }
                for (int i = 6; i < 10; i++)
                {
                    tableau[i] = 0; // 0 car réserver
                }


                //starting image
                for (int i = 10; i < 14; i++)
                {
                    tableau[i] = taille_du_offset[i - 10];
                }

                //40 giver value
                tableau[14] = 40;
                for (int i = 15; i < 18; i++)
                {
                    tableau[i] = 0;
                }


                for (int i = 18; i < 22; i++)
                {
                    tableau[i] = largeur_de_image[i - 18];
                }

                for (int i = 22; i < 25; i++)
                {
                    tableau[i] = hauteur_de_image[i - 22];
                }
                tableau[26] = 1;
                tableau[27] = 0;
                tableau[28] = (byte)(bpp); //typical value (but can be 34 and others
                tableau[29] = 0;//bpp < 32 l'autre byte est une vérif ou cas spéciaux ultra rares
                for (int i = 30; i < 33; i++)//compression can be 0,1,2, 3.... choose 0 like a normal image (so some image won't be properly read)
                {
                    tableau[i] = 0;
                }


                for (int i = 34; i < 37; i++)//c'est faux mais je en comprends pas pourquoi
                {
                    tableau[i] = sizeofimageConverted[i - 34];
                }


                //we decided 0 because it seems it is not affecting the image and we dont know how yet to have the size in meters of one pixel
                //horizontale resolution pixel per meter
                tableau[38] = 0;
                tableau[39] = 0;

                //verticale resolution pixel per meter
                tableau[42] = 0;
                tableau[43] = 0;

                //resolution of pixel per meters hear is real number of pixel per meter (can be 42...)
                tableau[54] = 0;
                //zone critique ou on enregistre les conversions de n bits vers 24 bits

                for (int i = 55; i < offset; i++)
                {
                    tableau[i] = images[i];
                }


                for (int i = 0; i < hauteur; i++)
                {
                    List<int> ligne_bits = new List<int>();
                    for (int j = 0; j < largeur; j++)
                    {
                        //convertion vers les bits
                        int[] stockage_memoire = Convertir_couleurs_en_binaire((int)(image[i, j].R), (int)(image[i, j].G), (int)(image[i, j].B), bpp);
                        for (int k = 0; k < bpp; k++)
                        {
                            ligne_bits.Add(stockage_memoire[k]);
                        }


                    }
                    //on rempli le dernier octet
                    while (ligne_bits.Count % 8 != 0)
                    {
                        ligne_bits.Add(0);
                    }
                    //on repasse tout en octet et on l'insère
                    for (int k = 0; k < ligne_bits.Count; k += 8)
                    {
                        int[] ligne = new int[8];
                        for (int l = 0; l < 8; l++)
                        {
                            ligne[l] = ligne_bits[k + l];
                        }


                        tableau[i * ((ligne_bits.Count / 8) - (((ligne_bits.Count + 3) / 8) % 4)) + (k / 8) + offset] = Convertir_bit_en_int(ligne);

                    }
                    //on rempli la dernière ligne
                    int manquant = 4 - (ligne_bits.Count / 8) % 4;
                    for (int j = 0; j < manquant; j++)
                    {
                        tableau[offset + i * (manquant + (ligne_bits.Count / 8)) + j + (ligne_bits.Count / 8)] = 0;
                    }


                }

                File.WriteAllBytes("./" + file + ".bmp", tableau);


            }
            public void From_Image_To_File(Pixel[,] tab, string file = "image_to_file", int bitsperpixel = 24)
            {
                int taille_ligne_oct = (tab.GetLength(1) * bitsperpixel / 8) + 4 - ((tab.GetLength(1) * bitsperpixel / 8) % 4);
                int debut = 54;
                if (bitsperpixel != 24)
                {
                    debut += (int)(Math.Pow(2, bitsperpixel + 2));
                }
                byte[] taille_du_offset = Convertir_Int_To_Endian(debut);
                byte[] largeur_de_image = Convertir_Int_To_Endian(tab.GetLength(1));
                byte[] hauteur_de_image = Convertir_Int_To_Endian(tab.GetLength(0));
                byte[] taille_du_fichier = Convertir_Int_To_Endian(debut + tab.GetLength(0) * taille_ligne_oct);//c'est faux (sauf pour les multpile de 8 != 1) mais ca veut dire le nombre d'octet qu'utilise une couleur
                byte[] sizeofimageConverted = Convertir_Int_To_Endian(tab.GetLength(0) * taille_ligne_oct);
                byte[] tableau = new byte[debut + tab.GetLength(0) * taille_ligne_oct];



                tableau[0] = 66; // Car 42 en héxadécimal donne 66 en décimal
                tableau[1] = 77; // Car 4D en héxadécimal donne 77 en décimal

                for (int i = 2; i < 6; i++)
                {
                    tableau[i] = taille_du_fichier[i - 2]; // De l'octet 2 à 6, taille du fichier
                }
                for (int i = 6; i < 10; i++)
                {
                    tableau[i] = 0; // 0 car réserver
                }


                //starting image
                for (int i = 10; i < 14; i++)
                {
                    tableau[i] = taille_du_offset[i - 10];
                }

                //40 giver value
                tableau[14] = 40;
                for (int i = 15; i < 18; i++)
                {
                    tableau[i] = 0;
                }


                for (int i = 18; i < 22; i++)
                {
                    tableau[i] = largeur_de_image[i - 18];
                }

                for (int i = 22; i < 25; i++)
                {
                    tableau[i] = hauteur_de_image[i - 22];
                }
                tableau[26] = 1;
                tableau[27] = 0;
                tableau[28] = (byte)(bitsperpixel); //typical value (but can be 34 and others
                tableau[29] = 0;//bitsperpixel < 32 l'autre byte est une vérif ou cas spéciaux ultra rares
                for (int i = 30; i < 33; i++)//compression can be 0,1,2, 3.... choose 0 like a normal image (so some image won't be properly read)
                {
                    tableau[i] = 0;
                }


                for (int i = 34; i < 37; i++)//c'est faux mais je en comprends pas pourquoi
                {
                    tableau[i] = sizeofimageConverted[i - 34];
                }


                //we decided 0 because it seems it is not affecting the image and we dont know how yet to have the size in meters of one pixel
                //horizontale resolution pixel per meter
                tableau[38] = 0;
                tableau[39] = 0;

                //verticale resolution pixel per meter
                tableau[42] = 0;
                tableau[43] = 0;

                //resolution of pixel per meters hear is real number of pixel per meter (can be 42...)
                tableau[54] = 0;
                //zone critique ou on enregistre les conversions de n bits vers 24 bits

                for (int i = 55; i < debut; i++)//à modifier si on fait une fonction de convertion de format
                {
                    tableau[i] = images[i];//on suppose qu'on ne change pas de format
                }

                remplir_tableau_byte(tableau, tab, debut, bitsperpixel);
                File.WriteAllBytes("./" + file + ".bmp", tableau);


                var process = new Process();


            }
            public void remplir_tableau_byte(byte[] tableau, Pixel[,] tab, int debut, int bitsperpixel)
            {
                for (int i = 0; i < tab.GetLength(0); i++)
                {
                    List<int> ligne_bits = new List<int>();
                    for (int j = 0; j < tab.GetLength(1); j++)
                    {
                        //convertion vers les bits
                        int[] stockage_memoire = Convertir_couleurs_en_binaire(tab[i, j].R, tab[i, j].G, tab[i, j].B, bitsperpixel);
                        for (int k = 0; k < bitsperpixel; k++)
                        {
                            ligne_bits.Add(stockage_memoire[k]);
                        }
                    }
                    //on rempli le dernier octet
                    while (ligne_bits.Count % 8 != 0)
                    {
                        ligne_bits.Add(0);
                    }

                    //on repasse tout en octet et on l'insère
                    for (int k = 0; k < ligne_bits.Count; k += 8)
                    {
                        int[] ligne = new int[8];
                        for (int l = 0; l < 8; l++)
                        {
                            ligne[l] = ligne_bits[k + l];
                        }

                        tableau[(i * ((ligne_bits.Count / 8) + 3 - (((ligne_bits.Count / 8) + 3) % 4)) + (k / 8) + debut)] = Convertir_bit_en_int(ligne);

                    }
                    //on rempli la dernière ligne
                    int manquant = 4 - (ligne_bits.Count / 8) % 4;
                    for (int j = 0; j < manquant; j++)
                    {
                        tableau[debut + i * (manquant + (ligne_bits.Count / 8)) + j + (ligne_bits.Count / 8)] = 0;

                    }
                }
            }
            #endregion
            public int[] Convertir_couleurs_en_binaire(int red, int green, int blue, int bitsperpixel)
            {
                int[] result = new int[bitsperpixel];
                int niveau_avancement = 0;
                //pour le rouge
                int bit_memoire = ((bitsperpixel - bitsperpixel % 3) / 3); //arrondit à l'inferieur
                                                                           //conversion pour garder une harmonie proche
                int division_couleur = (int)(Math.Pow(2, bit_memoire)) - 1;
                for (int i = division_couleur; i > 0; i--)//car 1+2+4+8... donc le max est impaire
                {
                    if (red + (255 / (2 * division_couleur)) >= (255 / division_couleur) * i)//car on a fait le choix du noir et du blanc 0 85 170 255 (4 valeures et division par 3) le + (division_couleur/2) permet d'arrondir 
                    {
                        int[] red_tab = Convertir_int_en_binaire(i);

                        for (int j = 0; j < bit_memoire; j++)
                        {
                            result[j] = red_tab[j];
                        }
                        break;
                    }
                }
                niveau_avancement += bit_memoire;
                //pour le vert
                bit_memoire = ((bitsperpixel - bitsperpixel % 3) / 3) + ((bitsperpixel % 3) / 2); //arrondit à l'inferieur j'ai pensé à (bpp/3)+(((bpp/3)%1)*2)-((((bpp/3)%1)*2)%1)
                division_couleur = (int)(Math.Pow(2, bit_memoire)) - 1;
                //conversion pour garder une harmonie proche
                for (int i = division_couleur; i > 0; i--)
                {
                    if (green >= (255 / division_couleur) * i)
                    {
                        int[] green_tab = Convertir_int_en_binaire(i);
                        for (int j = 0; j < bit_memoire; j++)
                        {
                            result[j + niveau_avancement] = green_tab[j];

                        }

                        break;
                    }
                }
                niveau_avancement += bit_memoire;
                //pour le bleu
                bit_memoire = ((bitsperpixel + 2) / 3) - (((bitsperpixel + 2) / 3) % 1); //arrondit au superieur
                                                                                         //conversion pour garder une harmonie proche
                division_couleur = (int)(Math.Pow(2, bit_memoire)) - 1;
                for (int i = division_couleur; i > 0; i--)
                {
                    if (blue >= (255 * i / division_couleur))
                    {
                        int[] blue_tab = Convertir_int_en_binaire(i);
                        for (int j = 0; j < bit_memoire; j++)
                        {
                            result[j + niveau_avancement] = blue_tab[j];

                        }
                        break;
                    }
                }
                return result;
            }
            public int[] Convertir_int_en_binaire(int nombre)
            {
                int[] binaire = new int[8];
                int puissance = 128;//car on est majoré par bpp = 24 soit 256/2 = 128
                for (int i = 0; i < 8; i++)
                {
                    if (nombre / puissance >= 1)
                    {
                        binaire[7 - i] = 1;
                        nombre -= puissance;
                    }
                    puissance = puissance / 2;
                }
                return binaire;
            }
            public int[] Convertir_octet_en_binaire(byte octetA, byte octetB = 0)//car nous convertissons les deux prochains octect du tableau et utilisons ce qui nous semble bon
            {

                int[] binaire = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int puissance = 128;
                for (int i = 0; i < 8; i++)
                {
                    if (octetA / puissance >= 1)
                    {
                        binaire[7 - i] = 1;
                        octetA -= (byte)(puissance);
                        puissance = puissance / 2;

                    }
                }
                puissance = 128;
                for (int i = 0; i < 8; i++)
                {
                    if (octetB / puissance >= 1)
                    {
                        binaire[15 - i] = 1;
                        octetB -= (byte)(puissance);
                        puissance = puissance / 2;

                    }
                }
                return binaire;
            }
            public int[] Convertir_octet_en_binaire_surun(byte octet)//car nous convertissons les deux prochains octect du tableau et utilisons ce qui nous semble bon
            {

                int[] binaire = { 0, 0, 0, 0, 0, 0, 0, 0 };
                int puissance = 128;
                for (int i = 0; i < 8; i++)
                {
                    if (octet / puissance >= 1)
                    {
                        binaire[7 - i] = 1;
                        octet -= (byte)(puissance);
                        puissance = puissance / 2;

                    }
                }

                return binaire;
            }

            public int Convertir_Endian_To_Int(byte[] tab)
            {
                int resul = 0;
                int puissance = 1;
                for (int i = 0; i < tab.Length; i++)
                {
                    resul += tab[i] * puissance;
                    puissance = puissance * 256;
                }
                return resul;
            }
         
        //new
        public void RotationV2(int angle = 70)
        {
            //initialisation
            double angleRadian = (angle % 360)*Math.PI/180;
            double cos = Math.Cos(angleRadian);
            double sin = Math.Sin(angleRadian);
            double cos_l1 = cos * image.GetLength(1);
            double cos_l2 = Math.Cos(angleRadian + (Math.PI / 2)) * image.GetLength(0);
            double sin_l1 = sin * image.GetLength(1);
            double sin_l2 = Math.Sin(angleRadian + (Math.PI / 2)) * image.GetLength(0);


            int dimension_largeur = (int)(Math.Abs(cos_l1) + Math.Abs(cos_l2))+1;//pour les arrondis au cas ou
            int dimension_hauteur = (int)(Math.Abs(sin_l1) + Math.Abs(sin_l2))+1;//pour les arrondits au cas ou
            int distance_a_ajouter_largeur = (int)((dimension_largeur -cos_l1  -cos_l2 )/2);
            int distance_a_ajouter_hauteur = (int)((dimension_hauteur - sin_l1 - sin_l2) / 2); 
            Pixel[,] tabOutput = new Pixel[dimension_hauteur, dimension_largeur];
            for (int i = 0; i < dimension_hauteur; i++)
            {
                for (int j = 0; j < dimension_largeur; j++)
                {
                    tabOutput[i, j] = new Pixel(0, 0, 0);
                }
            }
            //rotation
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    int height = (int)(cos*i+sin*j) + distance_a_ajouter_hauteur;
                    int width = (int)(cos*j-sin*i)+distance_a_ajouter_largeur;
                    tabOutput[height, width].equal_pixel_of(image[i,j]);
                }
            }
            for (int i = 0; i < tabOutput.GetLength(0); i++)
            {
                for (int j = 0; j < tabOutput.GetLength(1); j++)
                {
                    if (tabOutput[i, j].est_noir() && i >0 && i < tabOutput.GetLength(0)-1 && j > 0 && j < tabOutput.GetLength(1)-1)
                    {
                        Pixel construct = moyenne_carre(i, j, tabOutput);
                        tabOutput[i,j].equal_pixel_of(construct);
                    }
                }
            }
            From_Image_To_File(tabOutput, "rotation" + Convert.ToString(angle), 24);
        }
        public Pixel moyenne_carre(int i , int j, Pixel[,] tab)
        {
            int moyenne_red = 0;
            int moyenne_green = 0;
            int moyenne_blue = 0;
            for(int k =0; k < 3; k++)
            {
                for(int l =0; l<3; l++)
                {
                    if(!(tab[i + k - 1, j + l - 1].est_noir()))
                    {
                        moyenne_blue += tab[i + k - 1, j + l - 1].B;
                        moyenne_red += tab[i + k - 1, j + l - 1].R;
                        moyenne_green += tab[i + k - 1, j + l - 1].G;
                    }
                    
                }
                
            }
            return new Pixel((byte)(moyenne_red / 8), (byte)(moyenne_green / 8), (byte)(moyenne_blue / 8));
        }
        public void Rotation(Pixel[,] tab_rot, int angle)
            {

                angle = angle % 360;

                //resizing on prévoit le max
                //pour une image en rectangle ABCD :
                int AC = (int)(Math.Sqrt(Math.Pow(tab_rot.GetLength(0), 2) + Math.Pow(tab_rot.GetLength(1), 2)));//le +1 permet d'éviter les porblèmes de rétraicissement en double vers int et +1 car on fait une rotation sur un centre noir
                                                                                                                 //AC c'est la diagonale
                Pixel[,] final = new Pixel[2 * AC, 2 * AC];
                //enregistrement de l'image dans cette nouvelle image
                for (int i = 0; i < 2 * AC; i++)
                {
                    for (int j = 0; j < 2 * AC; j++)
                    {
                        final[i, j] = new Pixel(0, 0, 0);
                    }
                }

                //on se place au centre du cercle de rayon AC et on créé l'image -> on tournera autour de A
                for (int i = 0; i < tab_rot.GetLength(0); i++)
                {
                    for (int j = 0; j < tab_rot.GetLength(1); j++)
                    {

                        final[i + AC, j + AC] = tab_rot[i, j];//le +1 permet de laisser un point de rotation hors de l'image
                    }
                }
                //on dirait que final est à l'envers

                // From_Image_To_File(final, "resizing", bpp);
                //From_Image_To_File(final, "rotation", 24);//à remplacer par 24
                //rotation
                //x selon la largeur y selon la hauteur le point central ne bouge pas !
                // /!\ ne pas pour plus tard de créer un tableau support (si rotation inferieur à 30° on risque de copier des pixel puis les redéplacer -> perte de pixel)

                if (angle % 90 <= 2 || angle % 90 >= 88)
                {
                    rotation_coordonnees_4(final, angle, AC, tab_rot.GetLength(1), tab_rot.GetLength(0));
                }
                else
                {
                    rotation_coordonnees_continue(final, angle, AC, tab_rot.GetLength(1), tab_rot.GetLength(0));
                }

                From_Image_To_File(final, "rotation" + Convert.ToString(angle), 24);//à remplacer par 24

            }

            //on a fait deux fonctions pour les droites parallèle aux ordonnée de la forme x =3 (qu'on ne pouvait pas réécrire)
            public void rotation_coordonnees_4(Pixel[,] result, int angle, int AC, int celonX, int celonY)
            {
                // int signeX = (int)((((int)(Math.Cos(angle * Math.PI / 180) + 1.5) - 1) * 2 + 2) / 3) * 2 - 1;//-1, 1, 1
                //int signeY = (int)((((int)(Math.Sin(angle * Math.PI / 180) + 1.5) - 1) * 2 + 2) / 3) * 2 - 1;


                if (angle < 50)//soit 360
                {
                    int signeX = 1;
                    int signeY = -1;
                    for (int i = 0; i < celonX; i++)
                    {
                        for (int j = 0; j < celonY; j++)
                        {
                            result[AC + signeY * j, AC + signeX * i].R = result[AC + j, AC + i].R;
                            result[AC + signeY * j, AC + signeX * i].G = result[AC + j, AC + i].G;
                            result[AC + signeY * j, AC + signeX * i].B = result[AC + j, AC + i].B;
                        }
                    }
                }
                else if (angle < 100)
                {
                    int signeX = -1;
                    int signeY = 1;
                    for (int i = 0; i < celonY; i++)
                    {
                        for (int j = 0; j < celonX; j++)
                        {
                            result[AC + signeY * j, AC + signeX * i].R = result[AC + i, AC + j].R;
                            result[AC + signeY * j, AC + signeX * i].G = result[AC + i, AC + j].G;
                            result[AC + signeY * j, AC + signeX * i].B = result[AC + i, AC + j].B;
                        }
                    }
                }
                else if (angle < 200)
                {
                    int signeX = -1;
                    int signeY = -1;
                    for (int i = 0; i < celonX; i++)
                    {
                        for (int j = 0; j < celonY; j++)
                        {
                            result[AC + signeY * j, AC + signeX * i].R = result[AC + j, AC + i].R;
                            result[AC + signeY * j, AC + signeX * i].G = result[AC + j, AC + i].G;
                            result[AC + signeY * j, AC + signeX * i].B = result[AC + j, AC + i].B;
                        }
                    }
                }
                else//270
                {
                    int signeX = 1;
                    int signeY = -1;
                    for (int i = 0; i < celonX; i++)
                    {
                        for (int j = 0; j < celonY; j++)
                        {
                            result[AC + signeY * i, AC + signeX * j].R = result[AC + j, AC + i].R;
                            result[AC + signeY * i, AC + signeX * j].G = result[AC + j, AC + i].G;
                            result[AC + signeY * i, AC + signeX * j].B = result[AC + j, AC + i].B;
                        }
                    }
                }


                //ici on utilise le fait que ce soit une rotation parfaite (pas de problème continue etc...):


                for (int i = 0; i < celonY; i++)
                {
                    for (int j = celonX - 1; j >= 0; j--)
                    {
                        result[AC + i, AC + j].R = 0;
                        result[AC + i, AC + j].G = 0;
                        result[AC + i, AC + j].B = 0;

                    }
                }


            }
            public void rotation_coordonnees_continueBis(Pixel[,] result, int angle, int AC, int celonX, int celonY)
            {
                //y= ax+b
                (int Ax_norm, int Ay_norm) = rotation_coordonneesV2(angle, AC, AC, AC, AC);

                (int Bx_norm, int By_norm) = rotation_coordonneesV2(angle, AC, AC, AC + celonX - 1, AC);
                (int Dx_norm, int Dy_norm) = rotation_coordonneesV2(angle + 90, AC, AC, AC, AC + celonY - 1);//car on avancera le longe de la droite
                double droiteAB_a = 0.0;
                double droiteAB_b = 0.0;
                double droiteAD_a = 0.0;
                double droiteAD_b = 0.0;
                /*  Console.WriteLine(result[377, 377].ToString());
                  Console.WriteLine(result[377, 696].ToString());
                  Console.WriteLine(result[576, 696].ToString());
                  Console.WriteLine(result[576, 377].ToString());*/


                droiteAD_a = (Ay_norm - Dy_norm);
                droiteAD_a = droiteAD_a / (Ax_norm - Dx_norm);//pour mettre du double dans l'opération
                droiteAD_b = Ay_norm;
                droiteAD_b -= droiteAD_a * Ax_norm;
                //AD est perpendiculaire
                droiteAB_a = -1 / droiteAD_a;
                droiteAB_b = Ay_norm - droiteAB_a * Ax_norm;


                double LDy_norm = (Ay_norm - Dy_norm);
                LDy_norm = LDy_norm / celonY;
                double LDx_norm = (Ax_norm - Dx_norm);
                LDx_norm = LDx_norm / celonY;
                double LBx_norm = (Ax_norm - Bx_norm);
                LBx_norm = LBx_norm / celonX;
                double LBy_norm = (Ay_norm - By_norm);
                LBy_norm = LBy_norm / celonX;

                for (int compteurY = 0; compteurY < celonY; compteurY++)
                {

                    for (int compteurX = 0; compteurX < celonX; compteurX++)
                    {
                        double Mx_norm = Ax_norm - compteurY * LDx_norm - compteurX * LBx_norm;
                        double My_norm = Mx_norm * droiteAB_a + droiteAB_b;


                        //les choix :
                        int cX1 = (int)(Mx_norm - Mx_norm % 1);
                        int cX2 = (int)(Mx_norm + 1 - Mx_norm % 1);
                        int cY1 = (int)(My_norm - My_norm % 1);
                        int cY2 = (int)(My_norm + 1 - My_norm % 1);
                        if (Mx_norm - cX1 > Mx_norm - cX2)
                        {
                            int trans = cX1;
                            cX1 = cX2;
                            cX2 = trans;
                        }
                        if (My_norm - cY1 > My_norm - cY2)
                        {
                            int trans = cY1;
                            cY1 = cY2;
                            cY2 = trans;
                        }

                        if (result[cY1, cX1].ToString() == "000")
                        {
                            result[cY1, cX1].R = result[AC + compteurY, AC + compteurX].R;
                            result[cY1, cX1].G = result[AC + compteurY, AC + compteurX].G;
                            result[cY1, cX1].B = result[AC + compteurY, AC + compteurX].B;

                        }
                        else if (result[cY2, cX1].ToString() == "000")
                        {
                            result[cY2, cX1].R = result[AC + compteurY, AC + compteurX].R;
                            result[cY2, cX1].G = result[AC + compteurY, AC + compteurX].G;
                            result[cY2, cX1].B = result[AC + compteurY, AC + compteurX].B;

                        }
                        else if (result[cY1, cX2].ToString() == "000")
                        {
                            result[cY1, cX2].R = result[AC + compteurY, AC + compteurX].R;
                            result[cY1, cX2].G = result[AC + compteurY, AC + compteurX].G;
                            result[cY1, cX2].B = result[AC + compteurY, AC + compteurX].B;

                        }
                        else if (result[cY2, cX2].ToString() == "000")
                        {
                            result[cY2, cX2].R = result[AC + compteurY, AC + compteurX].R;
                            result[cY2, cX2].G = result[AC + compteurY, AC + compteurX].G;
                            result[cY2, cX2].B = result[AC + compteurY, AC + compteurX].B;

                        }

                        result[AC + compteurY, AC + compteurX].R = 0;
                        result[AC + compteurY, AC + compteurX].G = 0;
                        result[AC + compteurY, AC + compteurX].B = 0;
                        /*  
                          Console.WriteLine(result[377, 567].ToString());
                        */
                    }

                    //on les faits passer à la ranger de pixels suivante

                    droiteAB_b = (Ay_norm - LDy_norm * compteurY) - droiteAB_a * (Ax_norm - LDx_norm * compteurY);

                }
            }
            public void rotation_coordonnees_continue(Pixel[,] result, int angle, int AC, int celonX, int celonY)
            {
                //y= ax+b
                (int Ax_norm, int Ay_norm) = rotation_coordonneesV2(angle, AC, AC, AC, AC);

                (int Bx_norm, int By_norm) = rotation_coordonneesV2(angle, AC, AC, AC + celonX - 1, AC);
                (int Dx_norm, int Dy_norm) = rotation_coordonneesV2(angle + 90, AC, AC, AC, AC + celonY - 1);//car on avancera le longe de la droite
                double droiteAB_a = 0.0;
                double droiteAB_b = 0.0;
                double droiteAD_a = 0.0;
                double droiteAD_b = 0.0;
                /*  Console.WriteLine(result[377, 377].ToString());
                  Console.WriteLine(result[377, 696].ToString());
                  Console.WriteLine(result[576, 696].ToString());
                  Console.WriteLine(result[576, 377].ToString());*/


                droiteAD_a = (Ay_norm - Dy_norm);
                droiteAD_a = droiteAD_a / (Ax_norm - Dx_norm);//pour mettre du double dans l'opération
                droiteAD_b = Ay_norm;
                droiteAD_b -= droiteAD_a * Ax_norm;
                //AD est perpendiculaire
                droiteAB_a = -1 / droiteAD_a;
                droiteAB_b = Ay_norm - droiteAB_a * Ax_norm;


                double LDy_norm = (Ay_norm - Dy_norm);
                LDy_norm = LDy_norm / celonY;
                double LDx_norm = (Ax_norm - Dx_norm);
                LDx_norm = LDx_norm / celonY;
                double LBx_norm = (Ax_norm - Bx_norm);
                LBx_norm = LBx_norm / celonX;
                double LBy_norm = (Ay_norm - By_norm);
                LBy_norm = LBy_norm / celonX;

                for (int compteurY = 0; compteurY < celonY; compteurY++)
                {

                    for (int compteurX = 0; compteurX < celonX; compteurX++)
                    {
                        double Mx_norm = Ax_norm - compteurY * LDx_norm - compteurX * LBx_norm;
                        double My_norm = Mx_norm * droiteAB_a + droiteAB_b;


                        //les choix :
                        int cX1 = (int)(Mx_norm - Mx_norm % 1);
                        int cX2 = (int)(Mx_norm + 1 - Mx_norm % 1);
                        int cY1 = (int)(My_norm - My_norm % 1);
                        int cY2 = (int)(My_norm + 1 - My_norm % 1);
                        if (Mx_norm - cX1 > Mx_norm - cX2)
                        {
                            int trans = cX1;
                            cX1 = cX2;
                            cX2 = trans;
                        }
                        if (My_norm - cY1 > My_norm - cY2)
                        {
                            int trans = cY1;
                            cY1 = cY2;
                            cY2 = trans;
                        }

                        if (result[cY1, cX1].ToString() == "000")
                        {
                            result[cY1, cX1].R = result[AC + compteurY, AC + compteurX].R;
                            result[cY1, cX1].G = result[AC + compteurY, AC + compteurX].G;
                            result[cY1, cX1].B = result[AC + compteurY, AC + compteurX].B;

                        }
                        else if (result[cY2, cX1].ToString() == "000")
                        {
                            result[cY2, cX1].R = result[AC + compteurY, AC + compteurX].R;
                            result[cY2, cX1].G = result[AC + compteurY, AC + compteurX].G;
                            result[cY2, cX1].B = result[AC + compteurY, AC + compteurX].B;

                        }
                        else if (result[cY1, cX2].ToString() == "000")
                        {
                            result[cY1, cX2].R = result[AC + compteurY, AC + compteurX].R;
                            result[cY1, cX2].G = result[AC + compteurY, AC + compteurX].G;
                            result[cY1, cX2].B = result[AC + compteurY, AC + compteurX].B;

                        }
                        else if (result[cY2, cX2].ToString() == "000")
                        {
                            result[cY2, cX2].R = result[AC + compteurY, AC + compteurX].R;
                            result[cY2, cX2].G = result[AC + compteurY, AC + compteurX].G;
                            result[cY2, cX2].B = result[AC + compteurY, AC + compteurX].B;

                        }

                        result[AC + compteurY, AC + compteurX].R = 0;
                        result[AC + compteurY, AC + compteurX].G = 0;
                        result[AC + compteurY, AC + compteurX].B = 0;
                        /*  
                          Console.WriteLine(result[377, 567].ToString());
                        */
                    }

                    //on les faits passer à la ranger de pixels suivante

                    droiteAB_b = (Ay_norm - LDy_norm * compteurY) - droiteAB_a * (Ax_norm - LDx_norm * compteurY);

                }
            }

            public (int, int) rotation_coordonneesV2(int angle, int Ax_norm, int Ay_norm, int Mx_norm, int My_norm)
            {
                double bras = Math.Sqrt((Mx_norm - Ax_norm) * (Mx_norm - Ax_norm) + (Ay_norm - My_norm) * (Ay_norm - My_norm));//plus juste de mettre largeur ou hauteur de l'image

                double angle_final = (angle * Math.PI / 180);
                int Mx1_norm;
                int My1_norm;

                if (bras == 0)
                {
                    Mx1_norm = Mx_norm;
                    My1_norm = My_norm;
                }
                else
                {
                    Mx1_norm = Ax_norm + (int)(bras * Math.Cos(angle_final));
                    My1_norm = Ay_norm + (int)(bras * Math.Sin(angle_final));
                }

                return (Mx1_norm, My1_norm);


            }

            public byte[] Convertir_Int_To_Endian(int val)
            {
                byte[] tableau = new byte[4];
                int puissance = 256 * 256 * 256;
                for (int i = 3; i >= 0; i--)
                {
                    int coefficient = (val / puissance) - (val / puissance) % 1;
                    tableau[i] = (byte)(coefficient);
                    val = val - puissance * coefficient;
                    puissance = puissance / 256;
                }
                return tableau;
            }
            public void Noir_et_blanc()
            {
                for (int i = 0; i < image.GetLength(0); i++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        int moyenne = (image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                        if (moyenne < (255 / 2))
                        {
                            image[i, j].R = 0;
                            image[i, j].G = 0;
                            image[i, j].B = 0;
                        }
                        else
                        {
                            image[i, j].R = 255;
                            image[i, j].G = 255;
                            image[i, j].B = 255;
                        }
                    }
                }

            }
            public void Nuances_de_gris()
            {
                for (int i = 0; i < image.GetLength(0); i++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        int gris = (int)(image[i, j].R + image[i, j].G + image[i, j].B) / 3;
                        image[i, j] = new Pixel((byte)gris, (byte)gris, (byte)gris);
                    }
                }
            }
            public Pixel[,] Agrandir(int val_aggrandissement)
            {
                Pixel[,] newimage = new Pixel[image.GetLength(0)*val_aggrandissement, image.GetLength(1)*val_aggrandissement];
                for (int i = 0; i < image.GetLength(0); i++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        for (int a = 0; a < val_aggrandissement; a++)
                        {
                            for (int b = 0; b < val_aggrandissement; b++)
                            {
                                newimage[i * val_aggrandissement + a, j * val_aggrandissement + b] = image[i, j];
                            }
                        }
                    }
                }
                From_Image_To_File(newimage, "coco_Aggrandit");
                return newimage;
            }
            public Pixel[,] Retrecir(int val_retrecissement)
            {
                Pixel[,] newimage = new Pixel[image.GetLength(0) / val_retrecissement, image.GetLength(1) / val_retrecissement];
                for (int i = 0; i < newimage.GetLength(0); i++)
                {
                    for (int j = 0; j < newimage.GetLength(1); j++)
                    {
                        newimage[i, j] = image[i * val_retrecissement, j * val_retrecissement];
                    }
                }
                From_Image_To_File(newimage, "coco_Retrecit");
                return newimage;

            }
        public Pixel[,] Effet_Miroir()
        {
            Pixel[,] newimage = new Pixel[image.GetLength(0), image.GetLength(1)];
            for (int i = 0; i < newimage.GetLength(0); i++)
            {
                for (int j = 0; j < ((newimage.GetLength(1) + newimage.GetLength(1) % 2) / 2); j++)
                {
                    newimage[i, j] = image[i, newimage.GetLength(1) - 1 - j];
                    newimage[i, newimage.GetLength(1) - 1 - j] = image[i, j]; ;
                }
            }
            From_Image_To_File(newimage, "coco_Miroir");
            return newimage;
            
        }
        public Pixel[,] Effet_Miroir_bas_vers_haut()
        {
            Pixel[,] newimage = new Pixel[image.GetLength(0), image.GetLength(1)];

            for (int i = 0; i < newimage.GetLength(0); i++)
            {
                for (int j = 0; j < newimage.GetLength(1); j++)
                {

                    newimage[i, j] = image[newimage.GetLength(0) - 1 - i, j];
                }
            }
            From_Image_To_File(newimage, "coco_Miroir_bas_haut");
            return newimage;
        }
        public void devoiler_une_image(Pixel[,] result)
            {

                Pixel[,] image1 = new Pixel[result.GetLength(0), result.GetLength(1)];
                Pixel[,] image2 = new Pixel[result.GetLength(0), result.GetLength(1)];
                for (int i = 0; i < result.GetLength(0); i++)
                {
                    for (int j = 0; j < result.GetLength(1); j++)
                    {

                        image1[i, j] = new Pixel(devoilage_image(result[i, j].R, true), devoilage_image(result[i, j].G, true), devoilage_image(result[i, j].B, true));
                        image2[i, j] = new Pixel(devoilage_image(result[i, j].R, false), devoilage_image(result[i, j].G, false), devoilage_image(result[i, j].B, false));
                    }
                }
                From_Image_To_File(image1, "devoilage 1");
                From_Image_To_File(image2, "imageDOrigine");
            }
            public byte devoilage_image(int a, bool image)
            {
                int[] tab = Convertir_int_en_binaire(a);

                int[] tabr = new int[8];
                if (image)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        tabr[k+4] = tab[k + 4];
                        tabr[k] = 0;
                    }
                }
                else
                {
                    for (int k = 0; k < 4; k++)
                    {
                        tabr[k+4] = tab[k];
                        tabr[k] = 0;
                    }
                }

                int puissance = 1;
                int somme = 0;
                for (int k = 0; k < 8; k++)
                {
                    somme += tabr[k] * puissance;
                    puissance = puissance * 2;
                }
                return (byte)(somme);
            }
            public void cacher_une_image(Pixel[,] image1, Pixel[,] image2)
            {
                Pixel[,] result = new Pixel[image1.GetLength(0), image1.GetLength(1)];
                for (int i = 0; i < result.GetLength(0); i++)
                {
                    for (int j = 0; j < result.GetLength(1); j++)
                    {
                        result[i, j] = new Pixel(cachage_image(image1[i, j].R, image2[i, j].R), cachage_image(image1[i, j].G, image2[i, j].G), cachage_image(image1[i, j].B, image2[i, j].B));
                    }
                }
                From_Image_To_File(result, "cacher");

            }
            public byte cachage_image(int a, int b)
            {
                int[] tab1 = Convertir_int_en_binaire(a);
                int[] tab2 = Convertir_int_en_binaire(b);

                int[] tabr = new int[8];
                for (int k = 0; k < 4; k++)
                {
                    tabr[k] = tab1[k + 4];
                    tabr[k + 4] = tab2[k + 4];
                }
                int puissance = 1;
                int somme = 0;
                for (int k = 0; k < 8; k++)
                {
                    somme += tabr[k] * puissance;
                    puissance = puissance * 2;
                }
                return (byte)(somme);
            }
            public void histogramme(Pixel[,] tab)
            {
                int[,] result = new int[3, 256];

                for (int i = 0; i < tab.GetLength(0); i++)
                {
                    for (int j = 0; j < tab.GetLength(1); j++)
                    {
                        result[0, tab[i, j].R]++;
                        result[1, tab[i, j].G]++;
                        result[2, tab[i, j].B]++;
                    }
                }
                int max = 0;
                for (int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (result[j, i] > max)
                        {
                            max = result[j, i];
                        }
                    }
                }
                affichage_histogramme(result, max);
            }
            public void affichage_histogramme(int[,] histogramme, int taille)
            {
                Pixel[,] result = new Pixel[taille + 10, 256];
                for (int i = 0; i < histogramme.GetLength(1); i++)
                {
                    for (int j = 0; j < result.GetLength(0); j++)
                    {
                        result[j, i] = new Pixel(255, 255, 255);
                    }

                    //pour le rouge
                    for (int j = histogramme[0, i]; j < taille + 10; j++)
                    {
                        result[j, i].R = 0;

                    }
                    //pour le vert
                    for (int j = histogramme[1, i]; j < taille + 10; j++)
                    {
                        result[j, i].G = 0;

                    }
                    //pour le bleu
                    for (int j = histogramme[2, i]; j < taille + 10; j++)
                    {
                        result[j, i].B = 0;

                    }
                }
                From_Image_To_File(result, "histogramme_affichageadditif", 24);//à remplacer par 24

                Pixel[,] result_cote = new Pixel[taille + 10, 256 * 3];
                for (int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < result.GetLength(0); j++)
                    {
                        result_cote[j, i] = new Pixel(255, 255, 255);
                        result_cote[j, i + 256] = new Pixel(255, 255, 255);
                        result_cote[j, i + 2 * 256] = new Pixel(255, 255, 255);
                    }

                    //pour le rouge
                    for (int j = 0; j < histogramme[0, i]; j++)
                    {
                        result_cote[j, i].G = 0;
                        result_cote[j, i].B = 0;

                    }
                    //pour le vert
                    for (int j = 0; j < histogramme[1, i]; j++)
                    {
                        result_cote[j, i + 256].R = 0;
                        result_cote[j, i + 256].B = 0;

                    }
                    //pour le bleu

                    for (int j = 0; j < histogramme[2, i]; j++)
                    {
                        result_cote[j, i + 256 * 2].R = 0;
                        result_cote[j, i + 256 * 2].G = 0;

                    }
                }
                From_Image_To_File(result_cote, "histogramme_affichageacote", 24);//à remplacer par 24
            }
            public void convolution(Pixel[,] tab, int[,] kernel)
            {
                Pixel[,] result = new Pixel[tab.GetLength(0), tab.GetLength(1)];
                int ligne = kernel.GetLength(0) / 2;
                int colonne = kernel.GetLength(1) / 2;
                for (int i = 0; i < tab.GetLength(0); i++)
                {
                    for (int j = 0; j < tab.GetLength(1); j++)
                    {
                        int[,] carreR = new int[kernel.GetLength(0), kernel.GetLength(1)];
                        int[,] carreG = new int[kernel.GetLength(0), kernel.GetLength(1)];
                        int[,] carreB = new int[kernel.GetLength(0), kernel.GetLength(1)];
                        for (int k = i - ligne; k < i - ligne + kernel.GetLength(0); k++)
                        {
                            for (int l = j - colonne; l < j - colonne + kernel.GetLength(1); l++)
                            {
                                if (k >= 0 && l >= 0 && k < result.GetLength(0) && l < result.GetLength(1))
                                {
                                    carreR[k - i + ligne, l - j + colonne] = tab[k, l].R;
                                    carreG[k - i + ligne, l - j + colonne] = tab[k, l].G;
                                    carreB[k - i + ligne, l - j + colonne] = tab[k, l].B;
                                }

                            }
                        }

                        result[i, j] = new Pixel(kernel_cal(kernel, carreR), kernel_cal(kernel, carreG), kernel_cal(kernel, carreB));


                    }

                }
                From_Image_To_File(result, "convolution", 24);//à remplacer par 24
            }
            public byte kernel_cal(int[,] kernel, int[,] carre)
            {
                int somme = 0;
                for (int i = 0; i < kernel.GetLength(0); i++)
                {
                    for (int j = 0; j < kernel.GetLength(1); j++)
                    {
                        somme += kernel[i, j] * carre[i, j];
                    }
                }
                somme = somme / kernel.Length;
                if (somme < 0) { somme = 0; };
                if (somme > 255) { somme = 255; };
                return (byte)(somme);
            }
            public void Fractale_de_Mandelbrot(int itérations_max, int val_Red, int val_Green, int val_Blue)
            {
                Pixel pix = new Pixel(0, 0, 0);
                double abcisse_Mandelbrot_gauche = -2.1;
                double abcisse_Mandelbrot_droite = 0.6;
                double ordonnée_Mandelbrot_Bas = -1.2;
                double ordonnée_Mandelbrot_Haut = 1.2;
                int zoom = 100;
                int largeur = (int)((abcisse_Mandelbrot_droite - abcisse_Mandelbrot_gauche) * zoom);
                int hauteur = (int)((ordonnée_Mandelbrot_Haut - ordonnée_Mandelbrot_Bas) * zoom);
                Pixel[,] newimage = new Pixel[largeur, hauteur];
                for (int x = 0; x < largeur; x++)
                {
                    for (int y = 0; y < hauteur; y++)
                    {
                        double C_Reel = (x - largeur / 2.0) * 4.0 / largeur;
                        double C_Imaginaire = (y - hauteur / 2.0) * 4.0 / hauteur;
                        double Z_Reel = 0;
                        double Z_Imaginaire = 0;
                        int i = 0;
                        while (Z_Reel * Z_Reel + Z_Imaginaire * Z_Imaginaire < 4 && i < itérations_max)
                        {
                            double temporaire = Z_Reel;
                            Z_Reel = Z_Reel * Z_Reel - Z_Imaginaire * Z_Imaginaire + C_Reel;
                            Z_Imaginaire = 2 * Z_Imaginaire * temporaire + C_Imaginaire;
                            i = i + 1;

                        }
                        if (i != itérations_max)
                        {
                            newimage[x, y] = new Pixel((byte)(val_Red * i % 256), (byte)(val_Green * i % 256), (byte)(val_Blue * i % 256));
                        }
                        else
                        {
                            newimage[x, y] = pix;
                        }
                    }
                }
                From_Image_To_File(newimage, "Mandelbrot");
            }

        }


    }