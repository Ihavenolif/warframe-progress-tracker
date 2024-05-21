const lzma = require("lzma")
const fs = require("fs")

const dataLzma = fs.readFileSync("index_en.txt.lzma")
lzma.decompress(dataLzma, (decompressedData, err) => {
    console.log(decompressedData);
});