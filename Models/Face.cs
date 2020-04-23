using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UploadFilesServer.Models
{
        public partial class Face
        {
            [JsonProperty("faceId")]
            public Guid FaceId { get; set; }

            [JsonProperty("faceRectangle")]
            public FaceRectangle FaceRectangle { get; set; }

            [JsonProperty("faceAttributes")]
            public FaceAttributes FaceAttributes { get; set; }
        }

        public partial class FaceAttributes
        {
            [JsonProperty("smile")]
            public long Smile { get; set; }

            [JsonProperty("headPose")]
            public HeadPose HeadPose { get; set; }

            [JsonProperty("gender")]
            public string Gender { get; set; }

            [JsonProperty("age")]
            public long Age { get; set; }

            [JsonProperty("facialHair")]
            public FacialHair FacialHair { get; set; }

            [JsonProperty("glasses")]
            public string Glasses { get; set; }

            [JsonProperty("emotion")]
            public Emotion Emotion { get; set; }

            [JsonProperty("blur")]
            public Blur Blur { get; set; }

            [JsonProperty("exposure")]
            public Exposure Exposure { get; set; }

            [JsonProperty("noise")]
            public Noise Noise { get; set; }

            [JsonProperty("makeup")]
            public Makeup Makeup { get; set; }

            [JsonProperty("accessories")]
            public object[] Accessories { get; set; }

            [JsonProperty("occlusion")]
            public Occlusion Occlusion { get; set; }

            [JsonProperty("hair")]
            public Hair Hair { get; set; }
        }

        public partial class Blur
        {
            [JsonProperty("blurLevel")]
            public string BlurLevel { get; set; }

            [JsonProperty("value")]
            public double Value { get; set; }
        }

        public partial class Emotion
        {
            [JsonProperty("anger")]
            public long Anger { get; set; }

            [JsonProperty("contempt")]
            public long Contempt { get; set; }

            [JsonProperty("disgust")]
            public long Disgust { get; set; }

            [JsonProperty("fear")]
            public long Fear { get; set; }

            [JsonProperty("happiness")]
            public long Happiness { get; set; }

            [JsonProperty("neutral")]
            public long Neutral { get; set; }

            [JsonProperty("sadness")]
            public long Sadness { get; set; }

            [JsonProperty("surprise")]
            public long Surprise { get; set; }
        }

        public partial class Exposure
        {
            [JsonProperty("exposureLevel")]
            public string ExposureLevel { get; set; }

            [JsonProperty("value")]
            public double Value { get; set; }
        }

        public partial class FacialHair
        {
            [JsonProperty("moustache")]
            public double Moustache { get; set; }

            [JsonProperty("beard")]
            public double Beard { get; set; }

            [JsonProperty("sideburns")]
            public double Sideburns { get; set; }
        }

        public partial class Hair
        {
            [JsonProperty("bald")]
            public double Bald { get; set; }

            [JsonProperty("invisible")]
            public bool Invisible { get; set; }

            [JsonProperty("hairColor")]
            public HairColor[] HairColor { get; set; }
        }

        public partial class HairColor
        {
            [JsonProperty("color")]
            public string Color { get; set; }

            [JsonProperty("confidence")]
            public double Confidence { get; set; }
        }

        public partial class HeadPose
        {
            [JsonProperty("pitch")]
            public double Pitch { get; set; }

            [JsonProperty("roll")]
            public double Roll { get; set; }

            [JsonProperty("yaw")]
            public double Yaw { get; set; }
        }

        public partial class Makeup
        {
            [JsonProperty("eyeMakeup")]
            public bool EyeMakeup { get; set; }

            [JsonProperty("lipMakeup")]
            public bool LipMakeup { get; set; }
        }

        public partial class Noise
        {
            [JsonProperty("noiseLevel")]
            public string NoiseLevel { get; set; }

            [JsonProperty("value")]
            public double Value { get; set; }
        }

        public partial class Occlusion
        {
            [JsonProperty("foreheadOccluded")]
            public bool ForeheadOccluded { get; set; }

            [JsonProperty("eyeOccluded")]
            public bool EyeOccluded { get; set; }

            [JsonProperty("mouthOccluded")]
            public bool MouthOccluded { get; set; }
        }

        public partial class FaceRectangle
        {
            [JsonProperty("top")]
            public long Top { get; set; }

            [JsonProperty("left")]
            public long Left { get; set; }

            [JsonProperty("width")]
            public long Width { get; set; }

            [JsonProperty("height")]
            public long Height { get; set; }
        }

    public partial class IdentifyInput
    {
        [JsonProperty("personGroupId")]
        public string PersonGroupId { get; set; }

        [JsonProperty("faceIds")]
        public List<Guid> FaceIds { get; set; }

        [JsonProperty("confidenceThreshold")]
        public double ConfidenceThreshold { get; set; }
    }

    public partial class IdentifyOutput
    {
        [JsonProperty("faceId")]
        public Guid FaceId { get; set; }

        [JsonProperty("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public partial class Candidate
    {
        [JsonProperty("personId")]
        public Guid PersonId { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public partial class Person
    {
        [JsonProperty("personId")]
        public Guid PersonId { get; set; }

        [JsonProperty("persistedFaceIds")]
        public List<Guid> PersistedFaceIds { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("userData")]
        public object UserData { get; set; }
    }
}

