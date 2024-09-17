using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using GLTF.Extensions;
namespace GLTF.Schema
{
	public class EXT_volumetricvideo : IExtension
	{
		public const string EXTENSION_NAME = "EXT_volumetricvideo";
		public	MPEG_media mpeg_media;
		public	List<MeshId> meshAccessorList;
		public EXT_volumetricvideo()
        {
			meshAccessorList = new List<MeshId>(); 
		}
		public EXT_volumetricvideo(MPEG_media ext,  GLTFRoot root)
		{
			mpeg_media = ext;
			meshAccessorList = new List<MeshId>(); 
		}
		public EXT_volumetricvideo(MPEG_media ext, List<MeshId> meshAssID, GLTFRoot root)
		{
			mpeg_media = ext;
			meshAccessorList = new List<MeshId>(); 
		}
		public IExtension Clone(GLTFRoot gltfRoot)
		{
			var clone = new EXT_volumetricvideo();
			clone.mpeg_media = new MPEG_media(mpeg_media, gltfRoot); 
			for (int i = 0; i < meshAccessorList.Count; i++)
			{
				clone.meshAccessorList.Add(new MeshId(meshAccessorList[i], gltfRoot));
			}
			return clone;
		}
		/*
		public JProperty Serialize()
		{
			var obj = new JObject();
			var arr = new JArray();
			obj.Add("lights", arr);
			foreach (var light in lights)
			{
				var lightInfo = new JObject();
				arr.Add(lightInfo);
				lightInfo.Add("type", light.type);
				if (light.range > 0) lightInfo.Add("range", light.range);
				if (System.Math.Abs(light.intensity - 1f) > .0000001) lightInfo.Add("intensity", light.intensity);
				lightInfo.Add("name", light.Name ?? light.name);
				lightInfo.Add("color", new JArray(light.color.R, light.color.G, light.color.B));
				// TODO why is this not using the specific spotlight serializer/deserializer?
				if (light is GLTFSpotLight spotLight)
				{
					lightInfo.Add("spot", new JObject(
						new JProperty(nameof(spotLight.innerConeAngle), spotLight.innerConeAngle),
						new JProperty(nameof(spotLight.outerConeAngle), spotLight.outerConeAngle)));
				}
			}
			return new JProperty(EXTENSION_NAME, obj);
		}
		*/
		public JProperty Serialize()
		{
			JObject ext = new JObject();
			  
			if (mpeg_media != null)
			{ 
				ext.Add("mpeg_media", new JObject(mpeg_media.Serialize()));
			}

			if(meshAccessorList.Count>0)
            {
				var arr = new JArray();
				ext.Add("meshAccessorList", arr);
				/*
				ext.Add(new JProperty(
				VolumetricVideo_Factory.MESHACCESSORLIST,
					new JArray(meshAccessorList)
				));
				*/
				foreach (var light in meshAccessorList)
				{
					arr.Add(light.Id);
				}
			}
			return new JProperty(VolumetricVideo_Factory.EXTENSION_NAME, ext);
		}
	}
	public class VolumetricVideo_Factory : ExtensionFactory
	{
		public const string EXTENSION_NAME = "EXT_volumetricvideo";
		public const string MESHACCESSORLIST = "meshAccessorList";
		public const string MPEG_MEDIA = "mpeg_media";

		public VolumetricVideo_Factory()
		{ 
		}

		public override IExtension Deserialize(GLTFRoot root, JProperty extensionToken)
		{
			if (extensionToken != null)
			{
				var extension = new EXT_volumetricvideo(); 

				JToken mediaToken = extensionToken.Value[MPEG_MEDIA];
				extension.mpeg_media = MPEG_media_Factory.Deserialize(root, mediaToken);

				JToken meshAccessorListToken = extensionToken.Value[MESHACCESSORLIST]; 
				if (meshAccessorListToken != null) {
					JArray meshAccessorArray = meshAccessorListToken as JArray;
					foreach (var meshAcc in meshAccessorArray.Children())
					{
						extension.meshAccessorList.Add(MeshId.Deserialize(root, meshAcc.CreateReader()));
					}
				}
				return extension;
			}
			return null;
		}
	}  

	public class MPEG_media : IExtension
	{
		public string name;
		public List<Media> media;
		public MPEG_media() {
			name = "";
			media = new List<Media>();
		} 
		public MPEG_media(MPEG_media ext, GLTFRoot root)  {
			name = ext.name;
			media = new List<Media>(ext.media.Count);
			ext.media.ForEach((item) =>
			{
				media.Add(new Media(item, root));
			});

		}
		public MPEG_media(string nam,  Media  medi) {
			name = nam; 
			media = new List<Media>( );
			media.Add(medi); 
		}
		public MPEG_media(string nam, List<Media> medi)  {
			name = nam;
			media = medi;
			media = new List<Media>(medi.Count); 
			medi.ForEach((item) =>
			{
				media.Add(new Media(item));
			}); 
		} 
		public IExtension Clone(GLTFRoot gltfRoot)
		{
			var clone = new MPEG_media();
			for (int i = 0; i < media.Count; i++)
			{
				clone.media.Add(new Media(media[i], gltfRoot));
			}
			return clone; 
		} 
		public JProperty Serialize()
		{
			JObject jo = new JObject();
 
			jo.Add(new JProperty(MPEG_media_Factory.NAME, name)); 
			if (media != null)
			{ 
				var arr = new JArray();
				jo.Add("media", arr);
				foreach (var ms in media)
				{
					var mediaInfo = new JObject();
					arr.Add(mediaInfo);
					mediaInfo.Add("name", ms.name);
					mediaInfo.Add("startTime", ms.startTime);
					mediaInfo.Add("startTimeOffset", ms.startTimeOffset);
					mediaInfo.Add("endTimeOffset", ms.endTimeOffset);
					mediaInfo.Add("autoplay", ms.autoplay);
					mediaInfo.Add("autoplayGroup", ms.autoplayGroup);
					mediaInfo.Add("loop", ms.loop);
					mediaInfo.Add("controls", ms.controls); 
					var altArr = new JArray();
					mediaInfo.Add("alternatives", altArr);
					foreach (var alt in ms.alternatives)
					{
						var altObj = new JObject();
						altArr.Add(altObj);
						altObj.Add("uri", alt.uri);
						altObj.Add("mimeType", alt.mimeType);
						var trackArr = new JArray();
						altObj.Add("tracks", trackArr );
						foreach (var trac in alt.tracks)
						{
							var trackObj = new JObject();
							trackArr.Add(trackObj);
							trackObj.Add("track", trac.track);
							trackObj.Add("codec", trac.codecs); 
						} 
					} 
				} 
			}
			return new JProperty(MPEG_media_Factory.EXTENSION_NAME, jo);
		} 
	}

	public class MPEG_media_Factory : ExtensionFactory
	{
		public const string EXTENSION_NAME = "MPEG_media";
		public const string NAME = "name";
		public const string MEDIA = "media"; 

		public MPEG_media_Factory()
		{ 
		}
		public static MPEG_media Deserialize(GLTFRoot root, JToken token)
		{
			using (JsonReader reader = new JTokenReader(token))
			{
				return Deserialize(root, reader, token);
			}
		} 
		public static MPEG_media Deserialize(GLTFRoot root, JsonReader reader, JToken token)
		{
			var extension = new MPEG_media();

			if (reader.Read() && reader.TokenType != JsonToken.StartObject)
			{
				throw new Exception("Light must be an object.");
			}

			while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
			{
				var curProp = reader.Value.ToString();

				switch (curProp)
				{
					case NAME:
						extension.name = reader.ReadAsString();
						break; 
					case MEDIA: 
						extension.media = reader.ReadList(() => Media.Deserialize(root, reader));
						break;   
				}
			} 
			return extension;
		}
		public override  IExtension Deserialize(GLTFRoot root, JProperty extensionToken)
		{
			if (extensionToken != null)
			{
				var extension = new MPEG_media();

				JToken name = extensionToken.Value[NAME]; 

				if (name != null)
					extension.name = name.Value<string>();

				JToken mediasToken = extensionToken.Value[MEDIA];
				if (mediasToken != null)
				{ 
					JArray mediasArray = mediasToken as JArray;
					foreach (var mediaToken in mediasArray.Children())
					{
						extension.media.Add(Media.Deserialize(root, mediaToken));
					} 
				}
				return extension;
			} 
			return null; 
		}
	}

	public class Media :  GLTFChildOfRootProperty
	{ 
		public const string NAME = "name";
		public const string STARTTIME = "startTime";
		public const string STARTTIMEOFFSET = "startTimeOffset";
		public const string ENDTIMEOFFSET = "endTimeOffset";
		public const string AUTOPLAY = "autoplay";
		public const string AUTOPLAYGROUP = "autoplayGroup";
		public const string LOOP = "loop";
		public const string CONTROLS = "controls";
		public const string ALTERNATIVES = "alternatives";
		public string name;
		public double startTime;
		public double startTimeOffset;
		public double endTimeOffset;
		public bool autoplay;
		public int autoplayGroup;
		public bool loop;
		public bool controls;
		public List<Alternative> alternatives;
		public Media() {
			startTime = 0;
			startTimeOffset = 0;
			autoplay = true;
			loop = true;
			controls = false;
			alternatives = new List<Alternative>();
		}
		public Media(Media mediaInfo) 	{
			setMediaData(mediaInfo);
		}
		void setMediaData(Media mediaInfo) {
			if (mediaInfo == null) return;
			name = mediaInfo.name;
			startTime = mediaInfo.startTime;
			startTimeOffset = mediaInfo.startTimeOffset;
			endTimeOffset = mediaInfo.endTimeOffset;
			autoplay = mediaInfo.autoplay;
			autoplayGroup = mediaInfo.autoplayGroup;
			loop = mediaInfo.loop;
			controls = mediaInfo.controls;
			alternatives = new List<Alternative>(mediaInfo.alternatives.Count); 
			mediaInfo.alternatives.ForEach((item) =>
			{
				alternatives.Add(new Alternative(item));
			});
		}
		public Media(Media mediaInfo, GLTFRoot gltfRoot) : base(mediaInfo, gltfRoot) {
			setMediaData(mediaInfo);
		}
		public static Media Deserialize(GLTFRoot root, JsonReader reader )
		{
			var extension = new Media();
			if (reader.Read() && reader.TokenType != JsonToken.StartObject)
			{
				throw new Exception("Asset must be an object.");
			}
			while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
			{
				var curProp = reader.Value.ToString();

				switch (curProp)
				{
					case NAME:
						extension.name = reader.ReadAsString();
						break;
					case STARTTIME:
						extension.startTime = reader.ReadAsDouble().Value;
						break;
					case STARTTIMEOFFSET:
						extension.startTimeOffset = reader.ReadAsDouble().Value;
						break;
					case ENDTIMEOFFSET:
						extension.endTimeOffset = reader.ReadAsDouble().Value;
						break;
					case AUTOPLAY:
						extension.autoplay = reader.ReadAsBoolean().Value;
						break;
					case AUTOPLAYGROUP:
						extension.autoplayGroup = reader.ReadAsInt32().Value;
						break;
					case LOOP:
						extension.loop = reader.ReadAsBoolean().Value;
						break;
					case CONTROLS:
						extension.controls = reader.ReadAsBoolean().Value;
						break;
					case ALTERNATIVES:  
						extension.alternatives = reader.ReadList(() => Alternative.Deserialize(root, reader));
						break;
					default:
						extension.DefaultPropertyDeserializer(root, reader);
						break;
				}
			}

			return extension;
		}
		public static Media Deserialize(GLTFRoot root, JToken token)
		{
			using (JsonReader reader = new JTokenReader(token))
			{
				return Deserialize(root, reader  );
			}
		} 
	 
		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName(NAME);
			writer.WriteValue(name);
			
			if (startTime != 0)
			{
				writer.WritePropertyName(STARTTIME);
				writer.WriteValue(startTime);
			}
			if (startTimeOffset != 0)
			{
				writer.WritePropertyName(STARTTIMEOFFSET);
				writer.WriteValue(startTimeOffset);
			}
			writer.WritePropertyName(ENDTIMEOFFSET);
			writer.WriteValue(endTimeOffset);
            if (!autoplay) {
				writer.WritePropertyName(AUTOPLAY);
				writer.WriteValue(autoplay);  
            }
			writer.WritePropertyName(AUTOPLAYGROUP);
			writer.WriteValue(autoplayGroup); 
            if (!loop) {
				writer.WritePropertyName(LOOP);
				writer.WriteValue(loop);
			}
            if (controls)
            {
				writer.WritePropertyName(CONTROLS);
				writer.WriteValue(controls);
			}  
			writer.WritePropertyName(ALTERNATIVES);
			writer.WriteStartArray();
			foreach (var alt in alternatives)
            {
				alt.Serialize(writer);
            }
			writer.WriteEndArray();  
			base.Serialize(writer); 
			writer.WriteEndObject();
		}
		public JObject Serialize()
		{
			JTokenWriter writer = new JTokenWriter();
			Serialize(writer);
			return (JObject)writer.Token;
		} 


		public class Alternative : GLTFChildOfRootProperty
		{
			public const string URI = "uri";
			public const string MIMETYPE = "mimeType";
			public const string TRACK = "tracks"; 
			public string uri;
			public string mimeType;
			public List<Track> tracks;
			public Alternative() { }
			public void setup(Alternative mediaInfo) {
				if (mediaInfo == null) return;
				uri = mediaInfo.uri;
				mimeType = mediaInfo.mimeType;
				tracks = new List<Track>();
				foreach (var alt in mediaInfo.tracks) {
					tracks.Add(alt); }
			}
			public Alternative(string urii, string mime, Track  trac) {
				uri = urii;
				mimeType = mime;
				tracks = new List<Track>();
				tracks.Add(trac);
			}
			public Alternative(Alternative mediaInfo ) {
				setup(mediaInfo);
			} 
			public Alternative(Alternative mediaInfo, GLTFRoot gltfRoot) : base(mediaInfo, gltfRoot) {
				setup(mediaInfo);
			}
			override public void Serialize(JsonWriter writer)
			{
				writer.WriteStartObject();

				writer.WritePropertyName(URI);
				writer.WriteValue(uri);
				
				writer.WritePropertyName(MIMETYPE);
				writer.WriteValue(mimeType);
				  
				writer.WritePropertyName(TRACK);
				writer.WriteStartArray();
				foreach (var alt in tracks)
				{
					alt.Serialize(writer);
				}
				writer.WriteEndArray(); 
				 
				base.Serialize(writer);

				writer.WriteEndObject();
			}

			public JObject Serialize()
			{
				JTokenWriter writer = new JTokenWriter();
				Serialize(writer);
				return (JObject)writer.Token;
			}

			public static Alternative Deserialize(GLTFRoot root, JsonReader reader )
			{
				var extension = new Alternative();
				 
				if (reader.Read() && reader.TokenType != JsonToken.StartObject)
				{
					throw new Exception("Asset must be an object.");
				}

				while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
				{
					var curProp = reader.Value.ToString();

					switch (curProp)
					{
						case URI:
							extension.uri = reader.ReadAsString();
							break;
						case MIMETYPE:
							extension.mimeType = reader.ReadAsString();
							break;
						case TRACK:
							extension.tracks = reader.ReadList(() => Track.Deserialize(root, reader));   
							break;
						default:
							extension.DefaultPropertyDeserializer(root, reader);
							break;
					}
				} 
				return extension;
			}
			public static Alternative Deserialize(GLTFRoot root, JToken token)
			{
				using (JsonReader reader = new JTokenReader(token))
				{
					return Deserialize(root, reader );
				//	return Deserialize(root, reader, new JTokenReader(token));
				}
			}

			public class Track : GLTFProperty
			{
				private const string TRACK = "track"; // DASH: Using MPD Anchors (URL fragments) as defined in ISO/IEC 23009-1:2019:Annex C (Table C.1); ISOBMFF: URL fragments as specified in ISO/IEC 14496-12:2020:Annex C. SDP: stream identifier of the media stream as defined in ISO/IEC 20390-14:Annex C. When V3C data is referenced in the scene description document as in item in MPEG_media.alternative.tracks and the referenced item corresponds to an ISBOBMFF track, the following applies: for single-track encapsulated V3C data, the referenced track in MPEG_media shall be the V3C bitstream track. for multi-track encapsulated V3C data, the referenced track in MPEG_media shall be the V3C atlas track.
				private const string CODEC = "codec"; // When the track includes different types of codecs (e.g. the AdaptationSet includes Representations with different codecs), the codecs parameter may be signaled by comma-separated list of values of the codecs.
				public string track;
				public string codecs;    
				public Track()
				{
				}
				public Track(Track mediaInfo)
				{
					if (mediaInfo == null) return;
					track = mediaInfo.track;
					codecs = mediaInfo.codecs;
				} 
				public static Track Deserialize(GLTFRoot root, JsonReader reader)
				{
					var extension = new Track();

					if (reader.Read() && reader.TokenType != JsonToken.StartObject)
					{
						throw new Exception("Asset must be an object.");
					}

					while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
					{
						var curProp = reader.Value.ToString();

						switch (curProp)
						{
							case TRACK:
								extension.track = reader.ReadAsString();
								break;
							case CODEC:
								extension.codecs = reader.ReadAsString();
								break;  
						}
					} 
					return extension;
				}
				public static Track Deserialize(GLTFRoot root, JToken token)
				{
					using (JsonReader reader = new JTokenReader(token))
					{
						return Deserialize(root, reader);
					}
				}
				public override void Serialize(JsonWriter writer)
				{
					writer.WriteStartObject();

					writer.WritePropertyName(TRACK);
					writer.WriteValue(track);
					writer.WritePropertyName(CODEC);
					writer.WriteValue(codecs);
					base.Serialize(writer);
					writer.WriteEndObject();
				}
				public JObject Serialize()
				{
					JTokenWriter writer = new JTokenWriter();
					Serialize(writer);
					return (JObject)writer.Token;
				}
			}
		} 
	}
}
