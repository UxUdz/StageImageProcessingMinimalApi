const dropArea = document.querySelector('.drop-section');
const listSection = document.querySelector('.list-section');
const listContainer = document.querySelector('.list');
const fileSelector = document.querySelector('.file-selector');
const fileSelectorInput = document.querySelector('.file-selector-input');

const maxImages = 2;
let uploadedImagesCount = 0;

fileSelector.onclick = () => fileSelectorInput.click();
fileSelectorInput.onchange = () => {
    [...fileSelectorInput.files].forEach((file) => {
        if (typeValidation(file.type) && uploadedImagesCount < maxImages) {
            displayImage(file);
        }
    });
}

dropArea.ondragover = (e) => {
    e.preventDefault();
    handleDragItems(e.dataTransfer.items);
}

dropArea.ondragleave = () => dropArea.classList.remove('drag-over-effect');

dropArea.ondrop = (e) => {
    e.preventDefault();
    dropArea.classList.remove('drag-over-effect');
    handleDragItems(e.dataTransfer.items || e.dataTransfer.files);
}

function handleDragItems(items) {
    [...items].forEach((item) => {
        if (item.kind === 'file' && typeValidation(item.type)) {
            displayImage(item.getAsFile());
        }
    });
}

function typeValidation(type) {
    const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg'];
    return allowedTypes.includes(type);
}

function displayImage(file) {
    if (uploadedImagesCount >= maxImages) {
        alert('You can only upload a maximum of two images.');
        fileSelector.enabled = false;
        return;
    }

    listSection.style.display = 'block';
    uploadedImagesCount++;

    const li = document.createElement('li');
    li.classList.add('in-prog');
    li.innerHTML = `
        <div class="col">
            <img src="${URL.createObjectURL(file)}" alt="${file.name}" height="80px" width="80px"  style="border-radius: 15px;">
        </div>
        <div class="col">
            <div class="file-name">
                <div class="name">${file.name}</div>
                <span>0%</span>
            </div>
            <div class="file-progress">
                <span></span>
            </div>
            <div class="file-size">${(file.size / (1024 * 1024)).toFixed(2)} MB</div>
        </div>
        <div class="col">
            <svg xmlns="http://www.w3.org/2000/svg" class="cross" height="20" width="20">
                <path d="m5.979 14.917-.854-.896 4-4.021-4-4.062.854-.896 4.042 4.062 4-4.062.854.896-4 4.062 4 4.021-.854.896-4-4.063Z"/>
            </svg>
            <svg xmlns="http://www.w3.org/2000/svg" class="tick" height="20" width="20">
                <path d="m8.229 14.438-3.896-3.917 1.438-1.438 2.458 2.459 6-6L15.667 7Z"/>
            </svg>
        </div>
    `;
    listContainer.prepend(li);


    li.classList.add('complete');
    li.classList.remove('in-prog');

    li.querySelectorAll('span')[0].innerHTML = Math.round(percent_complete) + '%'
    li.querySelectorAll('span')[1].style.width = percent_complete + '%'

    li.querySelector('.cross').onclick = () => {
        li.remove();
        uploadedImagesCount--;
    };

    li.remove();
    uploadedImagesCount--;
}
