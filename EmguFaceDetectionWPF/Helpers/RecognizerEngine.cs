﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using EmguFaceDetectionWPF.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmguFaceDetectionWPF.Helpers
{
    public class RecognizerEngine
    {
        private FaceRecognizer faceRecognizer;
        private SqliteHelper db;
        private string recognizerPath;

        public RecognizerEngine(string recognizerFilePath)
        {
            recognizerPath = recognizerFilePath;
            db = new SqliteHelper();
            faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
        }

        public bool TrainRecognizer()
        {
            var allUsers = db.GetUsers(DbConstants.ALL_USERS_KEY);

            if (allUsers != null)
            {
                var faceImages = new List<Mat>();
                var faceLabels = new List<int>();
                for (int i = 0; i < allUsers.Count; i++)
                {
                    Stream stream = new MemoryStream();
                    stream.Write(allUsers[i].Face, 0, allUsers[i].Face.Length);
                    var faceImage = (new Bitmap(stream)).ToImage<Gray, byte>();
                    faceImages.Add(faceImage.Resize(100, 100, Inter.Cubic).Mat);
                    faceLabels.Add(allUsers[i].Uid);
                }
                faceRecognizer.Train(faceImages.ToArray(), faceLabels.ToArray());
                faceRecognizer.Write(recognizerPath);
            }

            return true;
        }

        public void LoadRecognizerData()
        {
            faceRecognizer.Read(recognizerPath);
        }

        public int RecognizeUser(Image<Gray, byte> userImage)
        {
            try
            {
                faceRecognizer.Read(recognizerPath);
                var res = faceRecognizer.Predict(userImage.Resize(100, 100, Inter.Cubic));
                return res.Label;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
