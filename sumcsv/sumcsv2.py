import csv
import sys 
import openpyxl

def readcsv(csvreader):
    """takes a file and two integers within the range of the file.
    precondition item and count < length of reader
    """
    data = []
    for i in csvreader:
        data.append(i)

    return data

def get_extention(filename:str):
    list = filename.split(".")
    if len(list) > 2:
        sys.stderr.write("invalid filename")
    else:
        return "."+list[1]
    
def load_xlsx(wb):
    
   
    ws = wb.get_active_sheet()
    info = []    
    data = []
     
    for row in ws.rows:
        
        data.append(info)
        info = []
        for cell in row:
            info.append(cell.value)
    data.remove(data[0])
    return data

def print_list(data:list,item,count):
    
    data, header = makeDict(data,item,count)

    data_s = sorted(data.keys())
    writer = csv.writer(sys.stdout, lineterminator='\n')
    print(header)
    for i in data_s:
        value = data[i]
        writer.writerow([i,value])


def makeDict(data:list,item,count):
    

    infoDict = {}
    if item >= len(data[0]) or item < 0 or count >= len(data[0]) or count < 0:
        
        sys.stderr.write("your index values are out of bounds")

    else:
        header = str(data[0][item]) + ',' + str(data[0][count])
        data.remove(data[0])
        for i in data:
            key = i[item]
            try:
                if key in infoDict:
                    infoDict[key] = float(infoDict[key]) + float(i[count])
                else:
                    infoDict[key] = float(i[count])
            except ValueError:
                if key in data:
                    infoDict[key]+=0
                else:
                    infoDict[key] = 0
        return infoDict,header


if __name__ == "__main__":

    if len(sys.argv) >= 4:
        filename = sys.argv[1]
        item = sys.argv[2]
        count = sys.argv[3]
        
        try:
            int(item)
            int(count)
        except ValueError:
            sys.stderr.write("You did not enter all the arguments correctly.\n Please use form: 'python sumcsv.py csv-file-name item-column count-column")

        try:
                
            if get_extention(filename) == ".csv": 

                with open(filename, newline='') as csvfile:
                    
                    
                    reader = csv.reader(csvfile)
                    data = readcsv(reader)
                    print_list(data,int(item),int(count))
            elif get_extention(filename):
                with open(filename, newline='') as xlsxfile:
                   
                    wb = openpyxl.load_workbook('babynames.xlsx')
                    data = load_xlsx(wb)
                    print_list(data,int(item),int(count))
                            
        except FileNotFoundError:
            sys.stderr.write("cannot read file")
       
            
         
    
    else:
        sys.stderr.write("You did not enter all the arguments correctly.\n Please use form: 'python sumcsv.py csv-file-name item-column count-column")