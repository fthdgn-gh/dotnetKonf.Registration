export class ApiResponse<T> {
    constructor(
        public isSucceeded: boolean = false, 
        public message: string = '', 
        public value:T = null) {
        
    }
}