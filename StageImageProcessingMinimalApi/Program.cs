using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV;
using System.Security.Cryptography;
using System.Drawing;
using StageImageProcessingMinimalApi.Utilities;
using Emgu.CV.Structure;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");  // Specify the desired port

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options.AddDefaultPolicy(builder => 
{ 
    builder.WithOrigins(
        "http://localhost:5000"
        );
}));

var app = builder.Build();
app.UseStaticFiles();

app.UseCors();

// Configure the HTTP request pipeline.


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


app.MapPost("/encrypt", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var imageFile = form.Files["image"];
    var userProvidedKey = form["encryptionKey"]; 
    
    if (imageFile == null || imageFile.Length == 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Please provide an image file");
        return;
    }

    using (var originalStream = imageFile.OpenReadStream())
    using (var encryptedStream = new MemoryStream())
    {
        byte[] key;
        if (!string.IsNullOrEmpty(userProvidedKey))
        {
            key = GetKeyFromString(userProvidedKey);
        }
        else
        {
            key = GenerateKey(); 
        }
        EncryptImage(originalStream, encryptedStream, key);

        var encryptedBytes = encryptedStream.ToArray();
        var encryptedBase64 = Convert.ToBase64String(encryptedBytes);
        var keyBase64 = Convert.ToBase64String(key);

        var response = new { EncryptedImage = encryptedBase64, EncryptionKey = keyBase64 };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapPost("/encryptSteganography", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var imageFile = form.Files["image"];
    var userProvidedKey = form["encryptionKey"]; 

    if (imageFile == null || imageFile.Length == 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Please provide an image file");
        return;
    }

    using (var originalStream = imageFile.OpenReadStream())
    using (var encryptedStream = new MemoryStream())
    {
        byte[] key;
        if (!string.IsNullOrEmpty(userProvidedKey))
        {
            key = GetKeyFromString(userProvidedKey);
        }
        else
        {
            key = GenerateKey(); 
        }
        EncryptImage(originalStream, encryptedStream, key);

        var encryptedBytes = encryptedStream.ToArray();
        var encryptedBase64 = Convert.ToBase64String(encryptedBytes);
        var keyBase64 = Convert.ToBase64String(key);

        var steganoEncryptedImage= SteganographyHelper.embedText(encryptedBase64);
        // Convert the Bitmap to a byte array
        using (MemoryStream ms = new MemoryStream())
        {
            steganoEncryptedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Choose appropriate format
            byte[] imageBytes = ms.ToArray();
            // Convert the byte array to a Base64 string
             encryptedBase64 = Convert.ToBase64String(imageBytes);
        }
        var response = new { EncryptedImage = encryptedBase64, EncryptionKey = keyBase64 };
        await context.Response.WriteAsJsonAsync(response);
    }
});


app.MapPost("/decrypt", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var encryptedBase64 = form["encryptedImageBase64"];
    var keyBase64 = form["decryptionKey"];

    if (string.IsNullOrEmpty(encryptedBase64) || string.IsNullOrEmpty(keyBase64))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Please provide an encrypted image and encryption key");
        return;
    }

    var encryptedBytes = Convert.FromBase64String(encryptedBase64);
    var key = GetKeyFromString(keyBase64);

    using (var decryptedStream = new MemoryStream())
    {
        DecryptImage(new MemoryStream(encryptedBytes), decryptedStream, key);

        var decryptedBytes = decryptedStream.ToArray();
        var decryptedBase64 = Convert.ToBase64String(decryptedBytes);

        var response = new { DecryptedBase64 = decryptedBase64};
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapPost("/decryptSteganography", async context =>
{
    var form = await context.Request.ReadFormAsync();
    IFormFile imageFile = form.Files["encryptedImage"];
    
    using (var stream = imageFile.OpenReadStream())
    {
        var encryptedBase64 = SteganographyHelper.extractText(new Bitmap(stream));
        var keyBase64 = form["decryptionKey"];

        if (string.IsNullOrEmpty(encryptedBase64) || string.IsNullOrEmpty(keyBase64))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Please provide an encrypted image and encryption key");
            return;
        }

        var encryptedBytes = Convert.FromBase64String(encryptedBase64);
        var key = GetKeyFromString(keyBase64);

        using (var decryptedStream = new MemoryStream())
        {
            DecryptImage(new MemoryStream(encryptedBytes), decryptedStream, key);

            var decryptedBytes = decryptedStream.ToArray();
            var decryptedBase64 = Convert.ToBase64String(decryptedBytes);
            
            
            var response = new { DecryptedBase64 = decryptedBase64};
            await context.Response.WriteAsJsonAsync(response); 
        }
    }
});

app.MapPost("/changedetection", async (HttpContext context) =>
        {
            var form = await context.Request.ReadFormAsync();
            IFormFile imageFile1 = form.Files["image1"];
            IFormFile imageFile2 = form.Files["image2"];
            double thresholdValue ;
            if(!double.TryParse(form["thresholdValue"],out thresholdValue))
            thresholdValue=30;

            if (imageFile1 == null || imageFile2 == null)
            {
                context.Response.StatusCode = 400; 
                await context.Response.WriteAsync("Both images must be provided.");
                return;
            }
            Mat image1 = new Mat();
            image1 = ReadImageFromFormFile(imageFile1);

            Mat image2 = new Mat();
            image2 = ReadImageFromFormFile(imageFile2);

            Mat result = new Mat();
            CvInvoke.AbsDiff(image1, image2, result);
            CvInvoke.CvtColor(result, result, ColorConversion.Bgr2Gray);
            CvInvoke.Threshold(result, result, thresholdValue, 255, ThresholdType.Binary);


            image2.SetTo(new MCvScalar(0, 0, 255), result);


            using (VectorOfByte vb = new VectorOfByte())
            {
                CvInvoke.Imencode(".jpg", image2, vb);
                using (MemoryStream ms = new MemoryStream(vb.ToArray()))
                {
                    var base64 = Convert.ToBase64String(ms.ToArray());
                    var response = new { OutputImageBase64 = base64};
                     await context.Response.WriteAsJsonAsync(response); 
                }
            }
        });

app.MapGet("/timenow", async (HttpContext context) =>
{
    using (Mat img = new Mat(100, 800, DepthType.Cv8U, 3))
    {
        img.SetTo(new Emgu.CV.Structure.MCvScalar(0, 0, 0));
        CvInvoke.PutText(
            img,
            String.Format("The time is: {0}", DateTime.Now.ToString()),
            new System.Drawing.Point(20, img.Height - 20),
            Emgu.CV.CvEnum.FontFace.HersheyPlain,
            2.0,
            new Emgu.CV.Structure.MCvScalar(255.0, 255.0, 255.0),
            1, LineType.EightConnected, false);

        context.Response.ContentType = "image/jpeg";

        using (VectorOfByte vb = new VectorOfByte())
        {
            CvInvoke.Imencode(".jpg", img, vb);
            using (MemoryStream ms = new MemoryStream(vb.ToArray()))
            {
                await ms.CopyToAsync(context.Response.Body);
            }
        }
    }
});

app.Run();

byte[] GenerateKey(){
    using (var rng = RandomNumberGenerator.Create())
    {
        var key = new byte[16]; 
        rng.GetBytes(key);
        return key;
    }
}
byte[] GetKeyFromString(string keyString)
{
    byte[] keyBytes;   
    try
    {
        keyBytes= Convert.FromBase64String(keyString);
    }
    catch (FormatException)
    {
        keyBytes=System.Text.Encoding.UTF8.GetBytes(keyString);
    }
    Array.Resize(ref keyBytes, 16);
    return keyBytes;
}
void EncryptImage(Stream input, Stream output, byte[] key)
{
    using (var aesAlg = Aes.Create())
    {
        if (aesAlg == null)
        {
            throw new InvalidOperationException("AES algorithm not supported on this platform");
        }

        aesAlg.Key = key;

        using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
        using (var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
        {
            output.Write(aesAlg.IV, 0, aesAlg.IV.Length); // Write IV to the beginning of the output stream
            input.CopyTo(cryptoStream);
        }
    }
}

void DecryptImage(Stream input, Stream output, byte[] key)
{
    using (var aesAlg = Aes.Create())
    {
        if (aesAlg == null)
        {
            throw new InvalidOperationException("AES algorithm not supported on this platform");
        }

        var iv = new byte[16];
        input.Read(iv, 0, iv.Length); // Read IV from the beginning of the input stream

        aesAlg.Key = key;
        aesAlg.IV = iv;

        using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
        using (var cryptoStream = new CryptoStream(output, decryptor, CryptoStreamMode.Write))
        {
            input.CopyTo(cryptoStream);
        }
    }
}

Mat ReadImageFromFormFile(IFormFile file)
{
    Mat image = new Mat();
    using (MemoryStream ms = new MemoryStream())
    {
        file.CopyTo(ms);
        byte[] imageData = ms.ToArray();
        CvInvoke.Imdecode(imageData, ImreadModes.Color, image);
    }
    return image;
}