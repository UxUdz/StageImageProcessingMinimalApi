function encryptImage() {
    const formData = new FormData(document.getElementById('encryptForm'));

    try {
        const response = await fetch('http://localhost:5000/encrypt', {
            method: 'POST',
            body: formData,
        });

        const result = await response.json();
        document.getElementById('encryptedImage').textContent = 'Encrypted Image: ' + result.EncryptedImage;
    } catch (error) {
        console.error('Error encrypting image:', error);
    }
}

function encryptSteganography() {
    const formData = new FormData(document.getElementById('steganographyForm'));

    try {
        const response = await fetch('http://localhost:5000/encryptSteganography', {
            method: 'POST',
            body: formData,
        });

        const result = await response.json();
        document.getElementById('steganographyEncryptedImage').textContent = 'Steganography Encrypted Image: ' + result.EncryptedImage;
    } catch (error) {
        console.error('Error encrypting image with steganography:', error);
    }
}

function decryptImage() {
    const formData = new FormData(document.getElementById('decryptForm'));

    try {
        const response = await fetch('http://localhost:5000/decrypt', {
            method: 'POST',
            body: formData,
        });

        const result = await response.text();
        document.getElementById('decryptedImage').textContent = 'Decrypted Image: ' + result;
    } catch (error) {
        console.error('Error decrypting image:', error);
    }
}

function decryptSteganography() {
    const formData = new FormData(document.getElementById('decryptSteganographyForm'));

    try {
        const response = await fetch('http://localhost:5000/decryptSteganography', {
            method: 'POST',
            body: formData,
        });

        const result = await response.text();
        document.getElementById('decryptedSteganographyImage').textContent = 'Decrypted Steganography Image: ' + result;
    } catch (error) {
        console.error('Error decrypting image with steganography:', error);
    }
}