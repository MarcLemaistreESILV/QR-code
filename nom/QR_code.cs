using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReedSolomon;

namespace nom
{
//eeee
    public class QR_code
    {
        private string mot;
        private int taille;
        private int dimension_QR_code;
        private int pixel_parcouru = 0;
        //entre un tableau de bool et un tableau int l'un est plus rapide l'autre prend moins de place

        public QR_code(string mot)
        {
            this.mot = mot;
            if (mot.Length > 25)
            {
                this.taille = 272;
                this.dimension_QR_code = 25;
            }
            else
            {
                this.dimension_QR_code = 21;
                this.taille = 152;
            }
        }

        public QR_code(Pixel[,] image)
        {
            this.dimension_QR_code = image.GetLength(0);
            int taille_mot = 0;
            if(dimension_QR_code == 25)
            {
                this.taille = 272;
                taille_mot = 359;
            }
            else
            {
                this.taille = 152;
                taille_mot = 220;
            }
            this.mot = QR_code_lecture_message(image, taille_mot);
        }

        public string Mot
        {
            get { return this.mot; }
            
        }

        public string QR_code_lecture_message(Pixel[,] tab, int taille_mot)
        {
           
            bool[] lecture_total_binaire = new bool[taille_mot];
            bool[] lecture_message_binaire = new bool[this.taille];
            bool[] lecture_reedSolomon_binaire;
            if(this.taille == 272)
            {
                lecture_reedSolomon_binaire = new bool[(taille_mot - this.taille) - (taille_mot - this.taille) % 8];
            }
            else
            {
                lecture_reedSolomon_binaire = new bool[56];
            }
            
            pixel_parcouru = 0;
            for (int colonne = tab.GetLength(1) - 1; colonne > tab.GetLength(1) - 8; colonne -= 2)
            {
                QR_code_lecture_message_colonne(lecture_total_binaire, tab, tab.GetLength(0) - 1, 8, colonne, -1);
                colonne -= 2;
                QR_code_lecture_message_colonne(lecture_total_binaire, tab, 9, tab.GetLength(0), colonne, 1);
            }
            //parcours  =96 /108
            //avant deuxième carré

            for (int colonne = tab.GetLength(1) - 9; colonne > 8; colonne -= 2)
            {
                QR_code_lecture_message_colonne(lecture_total_binaire, tab, tab.GetLength(0) - 1, 6, colonne, -1);
                QR_code_lecture_message_colonne(lecture_total_binaire, tab, 5, -1, colonne, -1);
                colonne -= 2;
                QR_code_lecture_message_colonne(lecture_total_binaire, tab, 0, 6, colonne, 1);
                QR_code_lecture_message_colonne(lecture_total_binaire, tab, 7, tab.GetLength(0), colonne, 1);
            }
           //parcours = 176

            //interstice
            QR_code_lecture_message_colonne(lecture_total_binaire, tab, tab.GetLength(0) - 9, 8, 8, -1);
            //couloir
            QR_code_lecture_message_colonne(lecture_total_binaire, tab, 9, tab.GetLength(0) - 8, 5, 1);
            QR_code_lecture_message_colonne(lecture_total_binaire, tab, tab.GetLength(0) - 9, 8, 3, -1);
            QR_code_lecture_message_colonne(lecture_total_binaire, tab, 9, tab.GetLength(0) - 8, 1, 1);
            //parcours =208
            for(int i =0; i < this.taille; i++)
            {
                lecture_message_binaire[i] = lecture_total_binaire[i];
            }
            for(int i = 0; i < lecture_reedSolomon_binaire.Length; i++)
            {
                lecture_reedSolomon_binaire[i] = lecture_total_binaire[i+this.taille];
            }
            byte[] message = convertion_boolTab_vers_byteTab(lecture_message_binaire);
            byte[] ecc = convertion_boolTab_vers_byteTab(lecture_reedSolomon_binaire);
            byte[] message_corrige = ReedSolomon.ReedSolomonAlgorithm.Decode(message, ecc, ErrorCorrectionCodeType.QRCode);
            string result = convertion_byteTab_vers_string(message_corrige);
            return result;
        }
        public string convertion_byteTab_vers_string(byte[] message_corrige)
        {
            int fin = 0;//correspond au nombre de caractère
            while(fin < message_corrige.Length && message_corrige[fin] != 17 && message_corrige[fin] != 236)
            {
                fin++;
            }
            fin--;
            if((fin * 8 - 9)%11 != 0)
            {
                fin = (((fin * 8 - 9) - (fin * 8 - 9) % 11) / 11)*2 + 1;
            }
            else
            {
                fin = (((fin * 8 - 9) - (fin * 8 - 9) % 11) / 11)*2;
            }
            string somme = "";
            int[] message_corrige_binaire = convertion_byte_vers_binaire_tab(message_corrige);
            for(int i = 13; i < message_corrige_binaire.Length; i+=11)
            {
                

                if(fin < 1)
                {
                    return somme;
                }
                else if(fin==1)
                {
                    int[] lettre_transition_bits = new int[6];
                    for (int j = 0; j < 6; j++)
                    {
                        lettre_transition_bits[j] = message_corrige_binaire[j + i];
                    }
                    int lettre_transition_int = convertion_bits_int_reverse(lettre_transition_bits);
                    somme += convertion_int_lettre_unique(lettre_transition_int);
                    return somme;
                }
                else
                {
                    int[] lettre_transition_bits = new int[11];
                    for (int j = 0; j < 11; j++)
                    {
                        lettre_transition_bits[j] = message_corrige_binaire[j + i];
                    }
                    int lettre_transition_int = convertion_bits_int_reverse(lettre_transition_bits);
                    somme += convertion_int_lettre_double(lettre_transition_int);
                    fin -= 2;
                }
            }
            return somme;
        }
        public string convertion_int_lettre_double(int lettre_int, bool est_seul = false)
        {
            
            int lettre2_int = lettre_int % 45;
            int lettre1_int = (lettre_int - lettre2_int) / 45;

            string lettre1 = convertion_int_lettre_unique(lettre1_int);
            
            if (!est_seul)
            {
                string lettre2 = convertion_int_lettre_unique(lettre2_int);
                return lettre1 + lettre2;
            }
            return lettre1;
        }
        public string convertion_int_lettre_unique(int result)
        {
            if (10 <= result && result < 36)//grandes lettres 65-90
            {
                result +=55;
            }
            else if (0 <= result && result<10)//nombre char => 48 à 57
            {
                result += 48;
            }

            else if (result == 36)// ' '
            {
                result = 32;
            }
            else if (result == 37)//$
            {
                result = 36;
            }
            else if (result == 38)//%
            {
                result = 37;
            }
            else if (result == 39)//*
            {
                result = 42;
            }
            else if (result == 40)// +
            {
                result = 43;
            }
            else if (result == 41)//-
            {
                result = 45;
            }
            else if (result == 42)//.
            {
                result = 46;
            }
            else if (result == 43)// /
            {
                result = 47;
            }
            else if (result == 44)// :
            {
                result = 58;
            }
            return Convert.ToString((char)(result));
        }
        public byte[] convertion_boolTab_vers_byteTab(bool[] tab)
        {
            byte[] result = new byte[tab.Length / 8];
            for(int i =0; i < tab.Length; i += 8)
            {
                int puissance = 128;
                byte somme = 0;
                for(int j =0; j < 8; j++)
                {
                    if (tab[i + j])
                    {
                        somme += (byte)(puissance);
                    }
                    puissance = puissance / 2;
                }
                result[i / 8] = somme;
            }
            return result;
        }
        public void QR_code_lecture_message_colonne(bool[] lecture_total_binaire, Pixel[,] tab, int debut, int fin, int colonne, int increment)
        {
            for(int i = debut; i != fin; i+=increment)
            {
                if(tab.GetLength(0) == 25 && ((colonne == 20 && i ==20) || (colonne == 18 && i == 15)))
                {
                    i = i+5*increment;
                }
                else if (tab.GetLength(0) == 25 && colonne == 16 && i == 20)
                {
                    for(int j =0; j < 5; j++)
                    {
                        lecture_total_binaire[pixel_parcouru] = tab[i+j*increment, colonne-1].est_noir();
                        pixel_parcouru++;
                    }
                    i -= 5;
                }
                lecture_total_binaire[pixel_parcouru] = tab[i, colonne].est_noir();
                pixel_parcouru++;
                lecture_total_binaire[pixel_parcouru] = tab[i, colonne-1].est_noir();
                pixel_parcouru++;
            }
        }
        public Pixel[,] ecriture_QR_code(int[] message)
        {
            Pixel[,] result = new Pixel[dimension_QR_code, dimension_QR_code];
            structure_QR_code(result, dimension_QR_code);
            ecriture_masque(result);
           
            implantation_mot_QR_code(result, message);
           //noir = 1
            //affichage_console(result);
            return result;
        }
        public void ecriture_masque(Pixel[,] result)
        {
            int[] masque = { 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0 };
            for (int i =0; i < 6; i++)
            {
                colore(result[8, i], masque[i]);
                colore(result[5-i, 8], masque[i+9]);
            }
            colore(result[8, 7], masque[6]);
            colore(result[8, 8], masque[7]);
            colore(result[7, 8], masque[8]);
            for (int i = 0; i < 7; i++)
            {
                colore(result[8, result.GetLength(1)-8+i], masque[i+7]);
                colore(result[result.GetLength(0) - 1 - i, 8], masque[i]);
            }
            colore(result[8, result.GetLength(1) - 1], masque[14]);
            colore(result[8, result.GetLength(1) - 8], 1);
        }
        public void implantation_mot_QR_code(Pixel[,] tab, int[] mot)
        {
           
            pixel_parcouru = 0;
            //avant premier carré
            for (int colonne = tab.GetLength(1)-1; colonne > tab.GetLength(1) - 8; colonne -=2)
            {
                pixel_parcouru = monte_QR_code(tab, pixel_parcouru, colonne, tab.GetLength(0)-1, 8, mot);
                colonne -= 2;
                pixel_parcouru = descente_QR_code(tab, pixel_parcouru, colonne, 9, tab.GetLength(0), mot);
            }
            //avant deuxième carré
           
            for (int colonne = tab.GetLength(1) -9; colonne > 8; colonne -= 2)
            {
                pixel_parcouru = monte_QR_code(tab, pixel_parcouru, colonne, tab.GetLength(0)-1, 6, mot);
                pixel_parcouru = monte_QR_code(tab, pixel_parcouru, colonne, 5, -1, mot);
                colonne -= 2;
                pixel_parcouru = descente_QR_code(tab, pixel_parcouru, colonne, 0, 6, mot);
                pixel_parcouru = descente_QR_code(tab, pixel_parcouru, colonne, 7, tab.GetLength(0), mot);
            }

            //interstice
            pixel_parcouru = monte_QR_code(tab, pixel_parcouru, 8, tab.GetLength(0) - 9, 8, mot);
            //couloir
            pixel_parcouru = descente_QR_code(tab, pixel_parcouru, 5, 9, tab.GetLength(0) - 8, mot);
            pixel_parcouru = monte_QR_code(tab, pixel_parcouru, 3, tab.GetLength(0) - 9, 8, mot);
            pixel_parcouru = descente_QR_code(tab, pixel_parcouru, 1, 9, tab.GetLength(0) - 8, mot);
           
        }
        //complète le bianire est renvoit combien il en a complété (cas du bloc supplémentaire)
        //titre trompeur, on monte le QR_code mais on descent le tableau
        public int monte_QR_code(Pixel[,] tab, int pixel_parcouru, int colonne_QR_code, int debut_y, int fin_y, int[] mot)
        {
           
            for (int i = debut_y; i > fin_y; i--)
            {
                
                //peu optimiser mais permet facilement de sauter les blocs
                if (tab[i, colonne_QR_code].est_noir() && tab[i, colonne_QR_code - 1].est_noir())
                {
                    i -= 4;
                }else if(tab[i, colonne_QR_code].est_noir())
                {
                    for(int j =0; j < 5; j++)
                    {
                        colore(tab[i-j, colonne_QR_code], mot[pixel_parcouru]);
                        pixel_parcouru++;
                        
                    }
                    i -= 4;
                }
                else if(tab[i, colonne_QR_code].est_noir())
                {
                    for (int j = 0; j < 5; j++)
                    {
                        colore(tab[i-j, colonne_QR_code-1], mot[pixel_parcouru]);
                        pixel_parcouru++;
                    }
                    i -= 4;
                }
                else
                {
                    colore(tab[i, colonne_QR_code], mot[pixel_parcouru]);
                    colore(tab[i, colonne_QR_code - 1], mot[pixel_parcouru+1]);
                    pixel_parcouru+=2;
                }
            }
            return pixel_parcouru;
        }
        //rend noir si 1 ou blanc si 0
        public void colore(Pixel a, int couleur)
        {
            if(couleur != 1)
            {
                pixel_noir(a);
            }
        }
        //complète le binaire est renvoit combien il en a complété (cas du bloc supplémentaire)
        public int descente_QR_code(Pixel[,] tab, int pixel_parcouru, int colonne_QR_code, int debut_y, int fin_y, int[] mot)
        {
            
            for (int i = debut_y; i < fin_y; i++)
            {
                
                //peu optimiser mais permet facilement de sauter les blocs
                if (tab[i, colonne_QR_code].est_noir() && tab[i, colonne_QR_code + 1].est_noir())
                {
                  
                    i += 4;
                   
                }
                else if (tab[i, colonne_QR_code].est_noir())
                {
                  
                    for (int j = 0; j < 5; j++)
                    {
                        colore(tab[i + j, colonne_QR_code], mot[pixel_parcouru]);
                        pixel_parcouru++;

                    }
                    i += 4;
                   
                }
                else if (tab[i, colonne_QR_code - 1].est_noir())
                {
                  
                    for (int j = 0; j < 5; j++)
                    {
                        colore(tab[i + j, colonne_QR_code - 1], mot[pixel_parcouru]);
                        pixel_parcouru++;
                    }
                    i += 4;
                   
                }
                else
                {
                  
                    colore(tab[i, colonne_QR_code], mot[pixel_parcouru]);
                    if (pixel_parcouru == 351) { pixel_parcouru-=2; };//je ne comprends pas qu'on aut 359 emplacements disponibles pour 352 bits à coder
                    colore(tab[i, colonne_QR_code - 1], mot[pixel_parcouru+1]);
                    pixel_parcouru += 2;
                }
            }
            return pixel_parcouru;
        }
        public void structure_QR_code(Pixel[,] tab, int version)
        {
            for (int i = 0; i < version; i++)
            {
                for (int j = 0; j < version; j++)
                {
                    tab[i, j] = new Pixel(255, 255, 255);
                }
            }
            ecriture_carre(tab, (tab.GetLength(0) -7), 0);
           
            ecriture_carre(tab, 0, tab.GetLength(1) - 7);
           
            ecriture_carre(tab, 0, 0);
           
            if (tab.GetLength(0) == 25)
            {
              ecriture_carre_petit(tab, tab.GetLength(0) - 9, tab.GetLength(1) - 9);
            }
           
            barre(tab);
        }
        public void ecriture_carre(Pixel[,] tab, int x, int y)
        {
            for(int i = 0; i < 7; i++)
            {
                pixel_noir(tab[x, y + i]);
                pixel_noir(tab[x + i, y]);
                pixel_noir(tab[x+6, y + i]);
                pixel_noir(tab[x+i, y + 6]);

            }

            x += 2;
                      y += 2;
            for (int i = 0; i < 3; i++)
            {
                pixel_noir(tab[x, y + i]);
                pixel_noir(tab[x+1, y + i]);
                pixel_noir(tab[x+2, y + i]);
            }
        }
        public void ecriture_carre_petit(Pixel[,] tab, int x, int y)
        {
            for (int i = 0; i < 5; i++)
            {
                pixel_noir(tab[x+i, y+4]);
                pixel_noir(tab[x+i, y]);
                pixel_noir(tab[x+4, y + i]);
                pixel_noir(tab[x, y +i]);

            }
            pixel_noir(tab[x+2, y+2]);
        }
        public void barre(Pixel[,] tab)
        {
            for (int i = 8; i < tab.GetLength(0)-8; i+=2)
            {
                pixel_noir(tab[i,6]);
                pixel_noir(tab[6, i]);
            }
        }
        public void pixel_noir(Pixel a)
        {
            a.R = 0;
            a.B = 0;
            a.G = 0;
        }
        #region ecriture message QR code
        public int[] construction_message_QR_code()
        {
            int[] total_pour_QR_code = new int[this.taille];

            //pour la version
            total_pour_QR_code[0] = 0;
            total_pour_QR_code[1] = 0;
            total_pour_QR_code[2] = 1;
            total_pour_QR_code[3] = 0;

            //pour la taille
            int[] taille_bits = convertion_int_bits(mot.Length);
            for (int i = taille_bits.Length; i < 9; i++)
            {
                total_pour_QR_code[i + 4 - taille_bits.Length] = 0;
            }
            for (int i = 0; i < taille_bits.Length; i++)
            {
                total_pour_QR_code[i + 4 + 9 - taille_bits.Length] = taille_bits[taille_bits.Length - 1 - i];
            }

            //pour le mot
            int[] mot_avant_salomon = convertion_mot_bit();//sur 32 bit et non 11 mais le ushort sur 16 bits perd les opérateur c'est relou

            for (int i = 0; i < mot_avant_salomon.Length; i++)
            {
                total_pour_QR_code[13 + i] = mot_avant_salomon[i];
            }
            //(mot.Length-mot.Length%2)*11 + (mot.Length%2)*6
            int terminator = 0;
            //terminator
            for (int i = 13 + (mot_avant_salomon.Length); (i < taille); i++)
            {
                total_pour_QR_code[i] = 0;
                terminator++;
                if (terminator >= 4 && (i + 1) % 8 == 0)
                {
                    break;
                }
            }
            //remplissage
            int[] remplissage236 = { 1, 1, 1, 0, 1, 1, 0, 0 };
            int[] remplissage17 = { 0, 0, 0, 1, 0, 0, 0, 1 };
            for (int i = 13 + mot_avant_salomon.Length + terminator; i < taille; i += 8)
            {
                for (int j = 0; j < 8; j++)
                {
                    total_pour_QR_code[i + j] = remplissage236[j];
                }
                i += 8;

                if (i < taille)
                {

                    for (int j = 0; j < 8; j++)
                    {
                        total_pour_QR_code[i + j] = remplissage17[j];
                    }
                }
            }
            int[] correction;
            if (this.taille == 272)
            {
                correction = convertion_byte_vers_binaire_tab(ReedSolomon.ReedSolomonAlgorithm.Encode(convertion_binaire_vers_byte_tab(total_pour_QR_code), 10, ErrorCorrectionCodeType.QRCode));//mot et The number of error correction codewords desired.
            }
            else
            {
                correction = convertion_byte_vers_binaire_tab(ReedSolomon.ReedSolomonAlgorithm.Encode(convertion_binaire_vers_byte_tab(total_pour_QR_code), 7, ErrorCorrectionCodeType.QRCode));//mot et The number of error correction codewords desired.
            }
            //problème de comprhéention trop de place
            int[] result;
            if (taille == 272)
            {
                result = new int[359];
            }
            else
            {
                result = new int[216];
            }
          //  int[] result = new int[taille + correction.Length];
            for (int i = 0; i < taille; i++)
            {
                result[i] = total_pour_QR_code[i];
            }
            for (int i = 0; i < correction.Length; i++)
            {
                result[i + taille] = correction[i];
            }
                     
            return result;
        }
        public int[] convertion_byte_vers_binaire_tab(byte[] tab)
        {
            int[] result = new int[tab.Length * 8];
            for (int i = 0; i < tab.Length; i++)
            {
                int[] binaire_temporaire = convertion_byte_vers_binaire(tab[i]);
                for (int j = 0; j < 8; j++)
                {
                    result[i * 8 + j] = binaire_temporaire[j];
                }
            }
            return result;
        }
        //permet de faire passer les tableaux de bit en byte
        public byte[] convertion_binaire_vers_byte_tab(int[] tab)
        {
            byte[] result = new byte[(tab.Length + tab.Length % 8) / 8];
            
            for (int i = 0; i < tab.Length; i += 8)
            {
                int[] byte_temporaire = new int[8];
                for (int j = 0; j < 8; j++)
                {
                    byte_temporaire[j] = tab[i + j];
                }
                result[i/8] = convertir_binaire_vers_byte(byte_temporaire);
            }
           
            return result;
        }
        public byte convertir_binaire_vers_byte(int[] byte_a_convertir)
        {

            int somme = 0;
            int puissance = 1;
            for (int i = 0; i < 8; i++)
            {
                somme += (byte)(byte_a_convertir[7 - i] * puissance);
                puissance = puissance * 2;
            }
            return (byte)(somme);
        }
        public int[] mot_uint_en_bit(uint[] tab)
        {
            int[] result = new int[11];
            List<int[]> result_temporaire = new List<int[]>();
            for (int i = 0; i < tab.Length; i++)
            {
                result_temporaire.Add(convertion_uint_bit_temporaire(tab[i]));
            }
            for (int i = 0; i < result_temporaire.Count; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    result[i * 9 + j] = result_temporaire[i][j];
                }
            }
            return result;
        }
        //en partant de 9!!!
        public int[] convertion_uint_bit_temporaire(uint nombre)
        {
            int[] result = new int[11];
            int puissance = (int)(Math.Pow(2, 10));
            for (int i = 0; i < 11; i++)
            {
                if (puissance <= nombre)
                {
                    nombre -= (uint)(puissance);
                    result[i] = 1;
                }
                else
                {
                    result[i] = 0;
                }
                puissance = puissance / 2;
            }
            return result;
        }
        public void affichage(int[] texte)
        {
            for (int i = 0; i < texte.Length; i += 8)
            {
                for (int j = 0; j < 8; j++)
                {
                    Console.Write(texte[i + j]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.ReadKey();
        }

        //converti les paires de lettres dans un tableaux mutplies des 11 bits
        public int[] convertion_mot_bit()
        {

            int[] final = new int[((mot.Length - mot.Length % 2) / 2) * 11 + (mot.Length % 2) * 6];

            for (int i = 0; i < mot.Length - (mot.Length % 2); i += 2)
            {
                char lettre1 = mot[i];
                char lettre2 = mot[i + 1];
                uint somme = (uint)(convertion_lettre_nombre(lettre1)) * 45 + (uint)(convertion_lettre_nombre(lettre2));

                // 01100001011 = 779
                int[] temporaire = convertion_uint_bit_temporaire(somme);

                for (int j = 0; j < 11; j++)
                {
                    final[(i / 2) * 11 + j] = temporaire[j];

                }

            }

            //la lettre impaire se faisant sur 6 bits
            if (mot.Length % 2 != 0)
            {
                char lettre1 = mot[mot.Length - 1];
                int[] temporaire = convertion_int_bits(convertion_lettre_nombre(lettre1));
                for (int j = 0; j < 6 - temporaire.Length; j++)
                {
                    final[((mot.Length - 1) / 2) * 11 + j] = 0;
                }
                for (int j = 0; j < temporaire.Length; j++)
                {
                    final[((mot.Length - 1) / 2) * 11 + (6 - temporaire.Length) + j] = temporaire[temporaire.Length - 1 - j];
                }
            }

            /* while(mot < taille/8)
             {
                 //ajouter : 11101100 00010001 
               //  int[] remplissage = { 1, 1, 1, 0, 1, 1, 0, 0 };
                 total.Add(236);
                 /*1);
                 total.Add(1);
                 total.Add(1);
                 total.Add(0);
                 total.Add(1);
                 total.Add(1);
                 total.Add(0);
                 total.Add(0);
                 if(total.Count < taille/8)
                 {
                     //int[] remplissage2 = { 0,0,0,1,0,0,0,1 };
                     total.Add(17);
                      total.Add(0);
                      total.Add(0);
                      total.Add(0);
                      total.Add(1);
                      total.Add(0);
                      total.Add(0);
                      total.Add(0);
                      total.Add(1);
                 }
             }*/
            return final;
        }

        public (byte, byte) int_to_byte(int nombre)
        {
            byte faible = (byte)(nombre % 255);
            nombre -= nombre % 255;
            nombre = nombre / 255;
            byte fort = (byte)(nombre);
            return (faible, fort);
        }
        //convertit sur 11 bit le nombre
        public int[] convertion_nombre_11bits(int somme)
        {
            int[] result = new int[11];
            int puissance = (int)(Math.Pow(2, 10));
            for (int i = 0; i < 11; i++)
            {
                if (somme > puissance)
                {
                    somme -= puissance;
                    result[i] = 1;
                }
                else
                {
                    result[i] = 0;
                }
                puissance = puissance / 2;
            }
            return result;
        }

        //renvoit le nombre associé à la lettre
        public int convertion_lettre_nombre(char lettre)
        {
            int result = (int)(lettre);

            if (96 < result && 124 > result)//petit lettre
            {
                result -= 87;
            }
            else if (64 < result && 92 > result)//grande lettre
            {
                result -= 55;
            }
            else if (47 < result && 58 > result)//nombre
            {
                result -= 48;
            }

            else if (result == 32)// ' '
            {
                result = 36;
            }
            else if (result == 36)//$
            {
                result = 37;
            }
            else if (result == 37)//%
            {
                result = 38;
            }
            else if (result == 42)//*
            {
                result = 39;
            }
            else if (result == 43)// +
            {
                result = 40;
            }
            else if (result == 45)//-
            {
                result = 41;
            }
            else if (result == 46)//.
            {
                result = 42;
            }
            else if (result == 47)// /
            {
                result = 43;
            }
            else if (result == 58)// :
            {
                result = 44;
            }
            return result;
        }
        public int[] convertion_int_bits(int nombre)
        {

            int puissance = 1;
            int dimension = 0;
            while (nombre > puissance)
            {
                puissance = puissance * 2;
                dimension++;
            }
            puissance = puissance / 2;//comme on l'a dépassé!
            int[] result = new int[dimension];
            for (int i = 0; i < dimension; i++)
            {
                if (puissance <= nombre)
                {
                    nombre -= puissance;
                    result[dimension - 1 - i] = 1;
                }
                else
                {
                    result[dimension - 1 - i] = 0;
                }
                puissance = puissance / 2;
            }
            return result;
        }
        public int[] convertion_byte_vers_binaire(byte nombreA)
        {
            int nombre = (int)(nombreA);
            int puissance = (int)(Math.Pow(2, 7));
            int[] result = new int[8];
            for (int i = 0; i < 8; i++)
            {
                if (puissance <= nombre)
                {
                    nombre -= puissance;
                    result[i] = 1;
                }
                else
                {
                    result[i] = 0;
                }
                puissance = puissance / 2;
            }
            return result;
        }
        public int[] convertion_byte_bits(byte nombreA)
        {
            int nombre = (int)(nombreA);
            int puissance = 1;
            int dimension = 1;
            while (nombre >= puissance)
            {
                puissance = puissance * 2;
                dimension++;
            }
            int[] result = new int[dimension];
            for (int i = 0; i < dimension; i++)
            {
                if (puissance < nombre)
                {
                    nombre -= puissance;
                    result[i] = 1;
                }
                else
                {
                    result[i] = 0;
                }
                puissance = puissance / 2;
            }
            return result;
        }
        public int convertion_bits_int(int[] bits)
        {
            int somme = 0;
            int puissance = 1;
            for (int i = 0; i < bits.Length; i++)
            {
                somme += bits[i] * puissance;
                puissance = puissance * 2;
            }
            return somme;
        }
        public int convertion_bits_int_reverse(int[] bits)
        {
            int somme = 0;
            int puissance = (int)(Math.Pow(2,bits.Length-1));
            for (int i = 0; i < bits.Length; i++)
            {
                somme += bits[i] * puissance;
                puissance = puissance / 2;
            }
            return somme;
        }
        #endregion

        public void affichage(byte[] tab)
        {
            foreach(byte element in tab)
            {
                Console.Write(element + " ");
            }
            Console.WriteLine();
        }
        public void affichage_console(Pixel[,] tab)
        {
            for (int j = 0; j < tab.GetLength(0); j++)
            {
                for (int k = 0; k < tab.GetLength(0); k++)
                {
                    if (k % 2 == 1)
                    {
                        Console.Write(" ");
                    }
                    Console.Write(tab[j, k].ToString_noir());
                }
                Console.WriteLine();
            }
            Console.WriteLine("--------------------------------------");

            Console.ReadKey();
            Console.Clear();
        }
    }
}

