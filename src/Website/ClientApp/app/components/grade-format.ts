export class GradeFormatValueConverter {
    toView(value) {
        switch (value) {
            case -1: return "PreK"
            case 0: return "KG";
            case 1: return "1st";
            case 2: return "2nd";
            case 3: return "3rd";
            case 4: return "4th";
            case 5: return "5th";
            case 6: return "6th";
            case 7: return "7th";
            case 8: return "8th";
            case 9: return "9th";
            case 10: return "10th";
            case 11: return "11th";
            case 12: return "12th";
            default: return "";
        }
    }
}