import logo from './logo.svg';
import React from 'react';
import './App.css';

function App() {
  return (
    <div className="App">
      
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.js</code> and save to reload.
        </p>
        <p>
          Learn React
        </p>
      </header>
    </div>
  );
}

export default App;

class Square extends React.Component {
 
  render() {
    return (
      <button className="square" onClick={()=>this.props.onClick()}>
        {this.props.value}
      </button>
    );
  }
}

class Board extends React.Component {
  constructor(props){
    super(props);
    this.state = {
      currentPlayer: "X",
      boardState: Array(9)
    };
  }

  handleClick(i){
   
    if(this.state.currentPlayer === "X")
      this.setState({currentPlayer: "O"})
    else
      this.setState({currentPlayer: "X"})
    
    const squares = this.state.boardState.slice();
    if(squares[i]==null)
      squares[i] = this.state.currentPlayer === "X" ? "X":"O";
    this.setState({boardState: squares});
    
  }
  
 renderSquare(i) {
    return <Square value={this.state.boardState[i]} onClick={()=>this.handleClick(i)}/>;
  }
  
  render() {

    return (
      <div>
        <div className="status">Next player: {this.state.currentPlayer}</div>
        <div className="board-row">
          {this.renderSquare(0)}
          {this.renderSquare(1)}
          {this.renderSquare(2)}
        </div>
        <div className="board-row">
          {this.renderSquare(3)}
          {this.renderSquare(4)}
          {this.renderSquare(5)}
        </div>
        <div className="board-row">
          {this.renderSquare(6)}
          {this.renderSquare(7)}
          {this.renderSquare(8)}
        </div>
      </div>
    );
  }
}

class Game extends React.Component {
  render() {
    return (
      <div className="game">
        <div className="game-board">
          <Board />
        </div>
        <div className="game-info">
          <div>{/* status */}</div>
          <ol>{/* TODO */}</ol>
        </div>
      </div>
    );
  }
}

// ========================================

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(<Game />);
