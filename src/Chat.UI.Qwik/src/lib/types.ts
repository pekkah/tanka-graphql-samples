export interface Channel {
    id: number; 
    name: string; 
    description: string;
    messages: Message[];
}

export interface Message {
    id: number;
    text: string;
    timestampMs: string;
    timestamp: Date;
    sender: Sender;
  }

  export interface Sender {
    sub: string;
    name: string;
  }