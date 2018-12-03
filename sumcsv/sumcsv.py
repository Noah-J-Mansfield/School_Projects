import csv
import sys 

def readcsv(csvreader,item:int,count:int):
    """takes a file and two integers within the range of the file.
    precondition item and count < length of reader
    """

    headerstring = next(csvreader)
    data = {}
    if item >= len(headerstring) or item < 0 or count >= len(headerstring) or count < 0:
    
        sys.stderr.write("your index values are out of bounds")

    else:
        header = headerstring[item] + ',' + headerstring[count]
        for i in csvreader:
            key = i[item]
            try:
                if key in data:
                    data[key] = float(data[key]) + float(i[count])
                else:
                    data[key] = float(i[count])
            except ValueError:
                if key in data:
                    data[key]+=0
                else:
                    data[key] = 0
        return data,header

    


def orginize_list(data:dict,header):
    data_s = sorted(data.keys())
    writer = csv.writer(sys.stdout, lineterminator='\n')
    print(header)
    for i in data_s:
        value = data[i]
        writer.writerow([i,value])


if len(sys.argv) >= 4:
    filename = sys.argv[1]
    column = sys.argv[2]
    count = sys.argv[3]
    try:
        int(column)
        int(count)
    except ValueError:
        sys.stderr.write("please use integers for item-column")

    try:
        with open(filename, newline='') as csvfile:
            reader = csv.reader(csvfile)
            data, header = readcsv(reader,int(column),int(count))
            orginize_list(data,header)
    except FileNotFoundError:
        sys.stderr.write("cannot read file")



   
   
else:
    sys.stderr.write("You did not enter all the arguments.\n Please use form: 'python sumcsv.py csv-file-name item-column count-column")