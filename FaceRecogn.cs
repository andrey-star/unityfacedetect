using UnityEngine;
using System.Collections;

public partial class VideoCapture : MonoBehaviour
{



    //Face Recognition
    int FaceRecognition(int w, int h)
    {

        int imageAverage = 0;
        if (_CaptureCounter % updateFrequency == 0 && _CaptureCounter > updateFrequency && w != 0 && h != 0)
        {
            /*список изменений

			список важных, больших идей
			*/

            int width = w;
            int height = h;
            int maxNumber = 2;
            int starterRowNumber = 10;
            int starterRowAddition = 5;
            int row = height / (starterRowNumber + (starterRowAddition * 2 - 1)); //хорошо бы теперь определять не через rowAddition
            int rowAddition = starterRowAddition;
            int rowNumber = starterRowNumber;
            bool withStarters = true;

            if (facesize * 4 / 3 / row - 1 < starterRowNumber && facesize != 0 && starterRowAddition + starterRowNumber - facesize * 4 / 3 / row + levelOfRows > 0)
            {
                rowNumber = facesize * 4 / 3 / row;
                rowAddition = starterRowAddition + starterRowNumber - rowNumber;
                withStarters = false;
            }

            int[] average = new int[rowNumber];
            int[] faceSizeArray = new int[rowNumber];
            int bannedRows = 0;
            float updateFrequencyMultiplier = 0.015f;


            Graphics.CopyTexture(snap, snap_cor);
            Graphics.CopyTexture(snap, snap_bw);

            //one more levelOfRows fixing
            while (rowAddition + levelOfRows < 0)
            {
                levelOfRows++;
            }
            int sideLength = 8;
            bool[,] allowedForMax = new bool[(width / sideLength), (height / sideLength)];// можно сделать квадратик поменьше

            for (int i = 0; i < width / sideLength; i++)
            {
                for (int j = 0; j < height / sideLength; j++)
                {
                    float greyChangesAmount = 0;
                    for (int i1 = 0; i1 < sideLength; i1++)
                    {
                        for (int j1 = 0; j1 < sideLength; j1++)
                        {
                            float scale_prev = snap_prev.GetPixel(i * sideLength + i1, j * sideLength + j1).grayscale;
                            float scale_cur = snap.GetPixel(i * sideLength + i1, j * sideLength + j1).grayscale;
                            if (System.Math.Abs(scale_cur - scale_prev) > updateFrequency * updateFrequencyMultiplier//можно сделать зависимость от кол-ва изм пикселей (перебор 76800))))
                                || System.Math.Abs(snap_prev.GetPixel(i * sideLength + i1, j * sideLength + j1).r - snap.GetPixel(i * sideLength + i1, j * sideLength + j1).r) > 30
                                || System.Math.Abs(snap_prev.GetPixel(i * sideLength + i1, j * sideLength + j1).g - snap.GetPixel(i * sideLength + i1, j * sideLength + j1).g) > 30
                                || System.Math.Abs(snap_prev.GetPixel(i * sideLength + i1, j * sideLength + j1).b - snap.GetPixel(i * sideLength + i1, j * sideLength + j1).b) > 30
                                )
                            {
                                //snap_cor.SetPixel (i * sideLength + i1, j * sideLength + j1, Color.red);
                                greyChangesAmount++;
                            }
                        }
                    }
                    if (greyChangesAmount > sideLength * 2)
                    {//здесь *2 не обосновано, но из 100 я брал 10 и было неплохо, но и 20 хорошо
                        allowedForMax[i, j] = true;

                        for (int idraw = 0; idraw < sideLength; idraw++)
                        {
                            for (int jdraw = 0; jdraw < sideLength; jdraw++)
                            {
                                //	snap_cor.SetPixel (i * sideLength + idraw, j * sideLength + jdraw, Color.red);
                            }
                        }

                    }
                }
            }

            //	snap_cor.Apply ();


            rowAddition += levelOfRows;
            //Debug.Assert (rowAddition >= 0);
            for (int irow = rowAddition * row; irow / row < rowNumber + rowAddition; irow += row)
            {
                float[] greys = new float[width];
                float[] greys_prev = new float[width];
                float[] der = new float[width];
                float[] max = new float[maxNumber];
                int[] maxIcol = new int[maxNumber];
                int summMaxIcol = 0;
                for (int icol = 0; icol < greys.Length; icol++)
                {
                    greys[icol] = snap.GetPixel(icol, irow).grayscale;

                }

                der = Derivatives(width, greys);

                for (int i = 0; i < der.Length; i++)
                {
                    der[i] = System.Math.Abs(der[i]);
                }
                if (_CaptureCounter != updateFrequency)
                {
                    for (int icol = 0; icol < greys.Length; icol++)
                    {
                        greys_prev[icol] = snap_prev.GetPixel(icol, irow).grayscale;
                    }
                    float flag_ones = 0;
                    for (int i = 0; i < greys.Length; i++)
                    {
                        if ((System.Math.Abs(greys[i] - greys_prev[i]) > updateFrequency * updateFrequencyMultiplier //вопрос с изменением цвета открыт
                                                                                                                     /*
                                                                                                                     || System.Math.Abs(snap_prev.GetPixel(i, irow).r - snap.GetPixel(i, irow).r) > 30
                                                                                                                     ||System.Math.Abs(snap_prev.GetPixel(i, irow).g - snap.GetPixel(i, irow).g) > 30
                                                                                                                     || System.Math.Abs(snap_prev.GetPixel(i, irow).b - snap.GetPixel(i, irow).b) > 30
                                                                                                                     */
                        ) && allowedForMax[(int)(i / sideLength), (int)irow / sideLength])
                        {
                            greys_prev[i] = 1;
                        }
                        else
                        {
                            greys_prev[i] = 0;
                        }
                    }
                    //отображение измененных пикселей
                    /*
                    for (int i = 0; i < greys.Length; i++)
                    {
                        if (greys_prev[i] == 1)
                        {
                            snap_cor.SetPixel(i, irow, Color.white);
                        }
                        else
                        {
                            snap_cor.SetPixel(i, irow, Color.black);
                        }
                    }
                    snap_cor.SetPixel(0, 0, Color.red);
                    */


                }



                bool banned = false;
                for (int i = 0; i < maxNumber; i++)
                {
                    for (int j = 0; j < der.Length; j++)
                    {
                        if (greys_prev[j] == 0 && _CaptureCounter != updateFrequency)
                        {
                            banned = true;
                        }
                        if (banned)
                        {
                            banned = false;
                            continue;
                        }
                        for (int k = 0; k < i; k++)
                        {
                            if (j == maxIcol[k] - 1 || j == maxIcol[k] + 1 ||
                                j == maxIcol[k] - 2 || j == maxIcol[k] + 2 ||
                                j == maxIcol[k] - 3 || j == maxIcol[k] + 3 ||
                                j == maxIcol[k] - 4 || j == maxIcol[k] + 4 ||
                                j == maxIcol[k] - 5 || j == maxIcol[k] + 5 ||
                                j == maxIcol[k] - 6 || j == maxIcol[k] + 6 ||
                                j == maxIcol[k] - 7 || j == maxIcol[k] + 7 ||
                                j == maxIcol[k] - 8 || j == maxIcol[k] + 8 ||
                                j == maxIcol[k] - 9 || j == maxIcol[k] + 9 ||
                                j == maxIcol[k] - 10 || j == maxIcol[k] + 10)
                            {
                                banned = true;
                                break;
                            }
                        }
                        if (banned)
                        {
                            banned = false;
                            continue;
                        }
                        if (i == 0)
                        {
                            if (max[0] < der[j])
                            {
                                max[0] = der[j];
                                maxIcol[0] = j;
                            }
                        }
                        else if (der[j] > max[i] && der[j] < max[i - 1])
                        {
                            if (System.Math.Abs(j - maxIcol[0]) > width / 10)
                            {// раньше было на 12, но этого мало (из эксперимента), хорошо бы опираться на facesize
                                max[i] = der[j];
                                maxIcol[i] = j;
                            }
                        }
                    }

                }
                for (int i = 0; i < maxNumber; i++)
                {
                    if (maxIcol[i] != 0 /*&& System.Math.Abs (maxIcol [0] - maxIcol [1]) > width / 10*/)
                    {
                        summMaxIcol += maxIcol[i];
                    }
                    else
                    {
                        //Debug.Log(175 + " " + maxIcol[i]);
                        average[(irow / row) - rowAddition] = 0;
                        break;
                    }
                    if (i == 1)
                    {
                        average[(irow / row) - rowAddition] = summMaxIcol / (maxNumber);
                    }
                }


                //  Debug.Log (average [0] + " ;" + average [1] + " ;" + average [2] + " ;" + average [3] + " ;" + average [4] + " ;" + average [5]);




                //отображение максимумов

                for (int i = 0; i < width; i++)
                {
                    //snap_cor.SetPixel (i, irow, Color.green);
                }

                for (int i = 0; i < maxNumber; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int k = -1; k <= 1; k++)
                        {
                            if (irow > 0 && irow < height && maxIcol[i] > 0 && maxIcol[i] < width)
                            {
                                //		snap_cor.SetPixel (maxIcol [i] + j, irow + k, Color.red);
                            }


                        }
                    }
                }
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (irow > 0 && irow < height && average[irow / (row) - rowAddition] > 0 && average[irow / (row) - rowAddition] < width)
                        {
                            //		snap_cor.SetPixel (average [irow / (row) - rowAddition] + j, irow + k, Color.yellow);
                        }
                    }
                }

                //snap_cor.SetPixel(0, 0, Color.red);

                faceSizeArray[(irow / row) - rowAddition] = Mathf.Abs(maxIcol[0] - maxIcol[1]);
                //for testing
                /*
				if (irow / (row) - rowAddition == 11 && _CaptureCounter % 36 == 0) {
					Debug.Log(maxIcol[0] + " " + maxIcol[1] + " " + irow);
				}
				if (irow / (row) - rowAddition == 11) {
					for (int j = -3; j <= 3; j++) {
						for (int k = -3; k <= 3; k++) {
							snap_cor.SetPixel (maxIcol [0] + j, 176 + k, Color.white);
							snap_cor.SetPixel (maxIcol [1] + j, 176 + k, Color.cyan);
						}
					}
				}
				//System.IO.File.WriteAllBytes(_SavePath + _CaptureCounter.ToString() + ".png", snap_cor.EncodeToPNG());
			*/

                //row work ended

            }


            //checking current rows of being far away from others
            bool[] banned_far_away = new bool[rowNumber];
            int zerosAmongRows = 0;
            for (int i = 0; i < rowNumber; i++)
            {
                if (average[i] == 0)
                {
                    zerosAmongRows++;
                }
            }
            if (rowNumber - 1 - zerosAmongRows > 0)
            {
                for (int i = 0; i < rowNumber; i++)
                {
                    if (average[i] == 0)
                    {
                        continue;
                    }
                    int summ_without = 0;
                    for (int j = 0; j < rowNumber; j++)
                    {
                        if (i != j)
                        {
                            summ_without += average[j];
                        }
                    }

                    if (System.Math.Abs((summ_without / (rowNumber - 1 - zerosAmongRows)) - average[i]) > (width / 10))
                    {// ?????????????
                     //Debug.Log(_CaptureCounter + " row " + i + " " + System.Math.Abs((summ_without / (rowNumber - 1))) + " " + average[i]);
                        banned_far_away[i] = true;
                    }
                    else
                    {
                        //Debug.Log(_CaptureCounter + " row " + i + " was not banned" + "(" + System.Math.Abs((summ_without / (rowNumber - 1))) + " ; " + average[i] + ")");
                    }
                }
            }
            else
            {
                for (int i = 0; i < rowNumber; i++)
                {
                    banned_far_away[i] = true;
                }
            }



            for (int irow = rowAddition * row; irow / row < rowNumber + rowAddition; irow += row)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (irow > 0 && irow < height && average[irow / (row) - rowAddition] > 0 && average[irow / (row) - rowAddition] < width)
                        {
                            if (banned_far_away[irow / row - rowAddition])
                            {
                                //	snap_cor.SetPixel (average [irow / (row) - rowAddition] + j, irow + k, Color.black);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < rowNumber; i++)
            {
                if (banned_far_away[i])
                {
                    //Debug.Log(283);
                    average[i] = 0;
                }
            }



            bannedRows = 0;
            for (int i = 0; i < rowNumber; i++)
            {
                if (average[i] != 0)
                {
                    imageAverage += average[i];
                }
                else
                {
                    bannedRows++;
                }
            }
            if (bannedRows > rowNumber * 3 / 4)
            {
                for (int irow1 = rowAddition * row; irow1 / row < rowNumber + rowAddition; irow1 += row)
                {
                    for (int i = 0; i < width; i++)
                    {
                        //snap_cor.SetPixel (i, irow1, Color.green);
                    }
                }
                //snap_cor.Apply();
                return 0;
            }
            imageAverage /= (rowNumber - bannedRows);
            //Debug.Log(imageAverage);

            for (int j = -2; j <= 2; j++)
            {
                for (int k = -2; k <= 2; k++)
                {
                    //	snap_cor.SetPixel (imageAverage + j, (rowAddition + (rowNumber - 1)/2)*row  + k, Color.magenta);
                }
            }


            //facesize
            //еще можно округлять в сторону предыдущего размера
            //все это должно работать хорошо, если не будет рандомных максимумов где попало(решено)
            //не стоит опираться на те значения, которые были до stop'а
            //совершать действия опираяь на  размер только при использовании нескольких его значений
            //массив с facesize'ами размера 3 или 5, на основании которых считается окончательный facesize
            int rowWithSize = 0; // for testing
            int counterForGoodAmountOfRows = 0;
            for (int i = rowNumber - 3; i > 0; i--)
            {
                if (average[i + 1] != 0)
                {
                    counterForGoodAmountOfRows = i - rowNumber / 4;
                    break;
                }
                else
                {
                    continue;
                }
            }
            for (int i = rowNumber - 3; i > counterForGoodAmountOfRows && i > 0; i--)
            {

                int next = faceSizeArray[i - 1];
                int current = faceSizeArray[i];
                int previous = faceSizeArray[i + 1];
                if (Mathf.Abs((previous + current) / 2 - next) > width / 20)
                {
                    continue;
                }
                if (Mathf.Abs((previous + next) / 2 - current) > width / 20)
                {
                    continue;
                }
                if (Mathf.Abs((next + current) / 2 - previous) > width / 20)
                {
                    continue;
                }
                int numberOfZeroRows = 0;
                if (average[i - 1] == 0)
                {
                    numberOfZeroRows++;
                }
                if (average[i] == 0)
                {
                    numberOfZeroRows++;
                }
                if (average[i + 1] == 0)
                {
                    numberOfZeroRows++;
                }
                if (numberOfZeroRows < 2 && (previous + current + next) / 3 >= width / 4)
                {//можно сделать бан при изменении больше чем на 10, (позже) плохая идея, надо что-то умнее
                    facesize = (previous + current + next) / 3 - numberOfZeroRows;
                    rowWithSize = i;
                    break;
                }
            }



            //fixing facesize using lastNfacesizes
            int facesizesActive = 0;
            if (facesize != 0)
            {
                facesizesActive++;
            }
            for (int i = 0; i < lastNfacesizes.Length - 1; i++)
            {
                lastNfacesizes[i] = lastNfacesizes[i + 1];
                if (lastNfacesizes[i] != 0)
                {
                    facesizesActive++;
                }
            }
            lastNfacesizes[lastNfacesizes.Length - 1] = facesize;

            if (_CaptureCounter > updateFrequency * (lastNfacesizes.Length + 1) && facesizesActive == lastNfacesizes.Length)
            {
                int summ = 0;
                for (int i = 0; i < lastNfacesizes.Length; i++)
                {
                    summ += lastNfacesizes[i];
                }
                facesize = summ / lastNfacesizes.Length;
            }
            //Debug.Log(facesize);

            //	Debug.Log ("size " + facesize + " row " + (rowWithSize + 1));
            //facesize end

            // надо попробовать что-то с красным цветом делать
            //upperBorder searching
            int upperBorder = height;
            for (int i = rowNumber - 1; i > 0; i--)
            {
                if (average[i] != 0)
                {
                    upperBorder = i;
                    break;
                }
                else
                {
                    continue;
                }
            }
            //lowerBorder searchings
            //searching for 3 maxes, then picking the lowest one as mouth, this is not supposed to work good cause it's just a try
            /*
			float[] lowerBorderSearcher = new float[height];
			for (int i = 0; i < height; i++) {
				lowerBorderSearcher [i] = snap.GetPixel (imageAverage, i).grayscale;
			}
			float[] derLowerBorderSearcher = Derivatives (lowerBorderSearcher.Length, lowerBorderSearcher);
			int mouthYwithThreeMaxes = 0;
			float [,] maxCol = new float[3,2];
			float forMaxHelper = 0;
			for (int max = 0; max < 3; max++) {
				forMaxHelper = float.MaxValue/2;
				bool banned = false;
				for (int i = row * rowAddition; i < height / 2; i++) {
					for (int k = 0; k < max; k++) {
						if (i == maxCol	 [k,0] - 1 || i == maxCol [k,0] + 1 ||
							i == maxCol	 [k,0] - 2 || i == maxCol [k,0] + 2 ||
							i == maxCol	 [k,0] - 3 || i == maxCol [k,0] + 3 ||
							i == maxCol	 [k,0] - 4 || i == maxCol [k,0] + 4 ||
							i == maxCol	 [k,0] - 5 || i == maxCol [k,0] + 5 ||
							i == maxCol	 [k,0] - 6 || i == maxCol [k,0] + 6 ||
							i == maxCol	 [k,0] - 7 || i == maxCol [k,0] + 7 ) {
							banned = true;
							break;
						}
					}
					if (banned) {
						banned = false;
						continue;
					}
					if (max == 0) {
						if (maxCol[0,1] > derLowerBorderSearcher [i]) {
							maxCol [0,0] = i;
							maxCol [0,1] = derLowerBorderSearcher [i];
						}
					}else if (derLowerBorderSearcher [i] < maxCol[max,1] && derLowerBorderSearcher[i] > maxCol[max - 1,1]) {//не модуль, так как нужна одна граница рта, а не каждый раз разная
						maxCol[max,0] = i;
						maxCol[max,1] = derLowerBorderSearcher [i];
					}
				}
			}

			forMaxHelper = float.MaxValue / 2;
			for (int i = 0; i < 3; i++) {
				if (forMaxHelper > maxCol [i, 0] && maxCol[i, 0] != 0) {
					forMaxHelper = maxCol [i, 0];
				}
			}
			mouthYwithThreeMaxes = (int)forMaxHelper;
			forMaxHelper = 0;
		//	Debug.Log (mouthYwithThreeMaxes + " with 3 maxes ");

			for (int i = 0; i < width; i++) {
				snap_cor.SetPixel (i, mouthYwithThreeMaxes - 1, Color.blue);
			}
			for (int i = 0; i < width; i++) {
				snap_cor.SetPixel (i, mouthYwithThreeMaxes, Color.blue);
			}
			for (int i = 0; i < width; i++) {
				snap_cor.SetPixel (i, mouthYwithThreeMaxes+1, Color.blue);
			}
			forMaxHelper = 0;
			*/

            //actual position searcher
            /*
			for (int i = imageAverage + (facesize / 4) - 5; i <= imageAverage + (facesize / 4) + 5; i++) {
				for (int j = mouthY; j < (upperBorder + rowAddition) * row; j++) {

				}
			}
			*/
            //eyes position searcher





            //tryna lay a rectangle on face

            int faceheight = facesize * 4 / 3;
            //up
            for (int i = imageAverage - facesize / 2; i <= imageAverage + facesize / 2; i++)
            {
                if (!snap_cor.GetPixel(imageAverage + facesize / 2, i).Equals(Color.yellow))
                {
                    SetBigPixel(snap_cor, i, (rowAddition + rowNumber - 1) * row, Color.blue);
                }
            }
            //down
            for (int i = imageAverage - facesize / 2; i <= imageAverage + facesize / 2; i++)
            {
                if (!snap_cor.GetPixel(imageAverage + facesize / 2, i).Equals(Color.yellow))
                {
                    SetBigPixel(snap_cor, i, (rowAddition + rowNumber - 1) * row - faceheight, Color.blue);
                }
            }
            //left
            for (int i = (int)((rowAddition + rowNumber - 1) * row - faceheight); i <= (int)((rowAddition + rowNumber - 1) * row); i++)
            {
                if (!snap_cor.GetPixel(imageAverage + facesize / 2, i).Equals(Color.yellow))
                {
                    SetBigPixel(snap_cor, imageAverage - facesize / 2, i, Color.blue);
                }
            }
            //right
            for (int i = (int)((rowAddition + rowNumber - 1) * row - faceheight); i <= (int)((rowAddition + rowNumber - 1) * row); i++)
            {
                if (!snap_cor.GetPixel(imageAverage + facesize / 2, i).Equals(Color.yellow))
                {
                    SetBigPixel(snap_cor, imageAverage + facesize / 2, i, Color.blue);
                }
            }

            //levelOfRows fixing
            if (!withStarters)
            {
                float averageForLevel = average[rowNumber - 1];
                float size = faceSizeArray[rowNumber - 1];
                if (averageForLevel == 0 && size == 0 && starterRowAddition + starterRowNumber - facesize * 4 / 3 / row + (levelOfRows - 1) > 0)
                {
                    levelOfRows--;
                }
                else if (averageForLevel != 0 && size > facesize * 0.75f && (rowAddition + rowNumber - 1) * row + levelOfRows + 1 < height - row
                        || starterRowAddition + starterRowNumber - facesize * 4 / 3 / row + levelOfRows < 0)
                {
                    levelOfRows++;
                }
            }
            //snap_cor.Apply();
            return imageAverage;
        }
        else
        {
            return -1;
        }
    }

    //transform.RotateAround(new Vector3(0, 0, 12), Vector3.up, imageAverage);

    public void SetBigPixel(Texture2D snap, int x, int y, Color color)
    {
        for (int j = -1; j <= 1; j++)
        {
            for (int k = -1; k <= 1; k++)
            {
               // snap.SetPixel(x + j, y + k, color);
            }
        }
    }
    public static float[] Derivatives(int count, float[] val)
    {

        //Вычисление производных
        float[] deriv = new float[count]; //Массив производных
        float s = 0, sk = 0;
        int k = 7; //Половина длины ядра для вычислений. Можно взять больше или меньше
        float[] kern = { -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7 }; //Ядро для получения производных.
        int nkern = kern.Length / 2; //Половина максимальной длины ядра. В данном случае это будет 7.
        for (int icol = 0; icol < count; icol++)
        {
            s = 0; sk = 0;
            for (int j = -k; j <= k; j++) //Вычисление производной в точке
            {
                if (icol + j >= 0 && icol + j < count && icol >= k && icol <= count - k) //Учитываем специфику начала и конца массива
                {
                    s += val[icol + j] * kern[j + nkern]; //Суммируем взвешенные значения вокруг точки
                    sk += (kern[j + nkern]) * (kern[j + nkern]); //Суммируем квадраты весов. Это для корректности значений на краях массива
                }
            }
            deriv[icol] = (sk > 0) ? s / sk : 0; //Получаем производную
        }
        return deriv;


    }

}
