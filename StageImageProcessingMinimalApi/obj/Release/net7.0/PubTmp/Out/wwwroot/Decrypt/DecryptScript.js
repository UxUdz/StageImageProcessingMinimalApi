function showContainer() {
var rightContainer = document.querySelector('.right-container');

rightContainer.style.display = (rightContainer.style.display === 'none' || rightContainer.style.display === '') ? 'flex' : 'none';
}