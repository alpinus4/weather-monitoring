import React, { Component } from 'react';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

export class FetchData extends Component {
  static displayName = FetchData.name;

  currentSearchState = {
    startDate: null,
    endDate: null,
    selectedIds: [],
    selectedTypes: [], 
  };

  constructor(props) {
    super(props);
    this.state = { 
      forecasts: [], 
      loading: true, 
      sortBy: null, 
      sortDirection: 'asc',
      startDate: null,
      endDate: null,
      selectedIds: [],
      selectedTypes: [], 
    };
  }

  
  componentDidMount() {
    this.populateWeatherData();
  }

  renderForecastsTable() {
    return (
      <div>
        <table className="table table-striped" aria-labelledby="tableLabel">
          <thead>
            <tr>
              <th>
                Date
                <button className="sortBtn" onClick={() => this.handleSort('timestamp', 'asc')}>
                  <span>▲</span>
                </button>
                <button className="sortBtn" onClick={() => this.handleSort('timestamp', 'desc')}>
                  <span>▼</span>
                </button>
              </th>
              <th>
                Id           
                <button className="sortBtn" onClick={() => this.handleSort('sensor_id', 'asc')}>
                  <span>▲</span>
                </button>
                <button className="sortBtn" onClick={() => this.handleSort('sensor_id', 'desc')}>
                  <span>▼</span>
                </button>
              </th>
              <th>
                Sensor Type 
                <button className="sortBtn" onClick={() => this.handleSort('type', 'asc')}>
                  <span>▲</span>
                </button>
                <button className="sortBtn" onClick={() => this.handleSort('type', 'desc')}>
                  <span>▼</span>
                </button>
              </th>
              <th>
                Value 
                <button className="sortBtn" onClick={() => this.handleSort('value', 'asc')}>
                  <span>▲</span>
                </button>
                <button className="sortBtn" onClick={() => this.handleSort('value', 'desc')}>
                  <span>▼</span>
                </button>
              </th>
            </tr>
          </thead>
          <tbody>
            {this.state.forecasts.map(forecast =>
              <tr key={forecast.timestamp}>
                <td>{forecast.timestamp}</td>
                <td>{forecast.sensor_id}</td>
                <td>{forecast.type}</td>
                <td>{forecast.value}</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    );
  }

  render() {
    let contents = this.state.loading ? <p><em>Loading...</em></p> : this.renderForecastsTable();

    return (
      <div>
        <h1 id="tableLabel">Weather monitoring</h1>
        <div className="input-row">
          <div className="input-section">
            <span>Date range:</span>
                    <DatePicker
                      onChange={(date) => this.handleDateChange(date)}
                      selectsRange={true}
                      startDate={this.state.startDate}
                      endDate={this.state.endDate}
                    />
          </div>
          
          <div className="input-section">
          <span>Sensor type:</span>
          {["temperature", "humidity", "pressure", "wind_speed"].map(type => (
                    <label key={type}>
                      <br></br>
                      {type}: 
                      <input 
                        type="checkbox" 
                        checked={this.state.selectedTypes.includes(type)} 
                        onChange={() => this.handleTypeCheckboxChange(type)}
                      />
                    </label>
                  ))}
                  </div>
          
          <div className="input-section">
          <span>Id:</span>
          {[0, 1, 2, 3].map(id => (
                    <label key={id}>
                      <br></br>
                      {id}: 
                      <input 
                        type="checkbox" 
                        checked={this.state.selectedIds.includes(id)} 
                        onChange={() => this.handleCheckboxChange(id)}
                      />
                    </label>
                  ))}
                  </div>
        </div>
        <button onClick={() => this.populateWeatherData()}>Filter</button>
        <br></br>
        {contents}
      </div>
    );
  }

  async populateWeatherData() {
    this.setState({ loading: true });
    var parameters = new URLSearchParams();
    if(this.state.sortBy) parameters.set("sortBy", this.state.sortBy)
    if(this.state.sortDirection) parameters.set("sortDirection", this.state.sortDirection)
    
    var obj = {ids: this.state.selectedIds, types: this.state.selectedTypes};
    if(this.state.startDate) obj.dateFrom = this.state.startDate.toISOString()
    if(this.state.endDate) obj.dateTo = this.state.endDate.toISOString()

    this.currentSearchState.selectedIds = this.state.selectedIds
    this.currentSearchState.selectedTypes = this.state.selectedTypes
    this.currentSearchState.dateFrom = this.state.dateFrom
    this.currentSearchState.dateTo = this.state.dateTo

    parameters.set("filters", JSON.stringify(obj));
    const response = await fetch('weatherforecast?'+parameters);
    const data = await response.json();
    this.setState({ forecasts: data, loading: false });
    
  }

  handleSort = async (columnName, direction) => {
    this.setState({ sortBy: columnName, sortDirection: direction }, () => {
      this.sortWeatherData(); // Fetch sorted data after updating the state
    });
    console.log(`Sort by ${columnName} ${direction}`);
  }

  async sortWeatherData() {
    this.setState({ loading: true });
    var parameters = new URLSearchParams();
    if(this.state.sortBy) parameters.set("sortBy", this.state.sortBy)
    if(this.state.sortDirection) parameters.set("sortDirection", this.state.sortDirection)

    var obj = {ids: this.currentSearchState.selectedIds, types: this.currentSearchState.selectedTypes};
    if(this.currentSearchState.startDate) obj.dateFrom = this.currentSearchState.startDate.toISOString()
    if(this.currentSearchState.endDate) obj.dateTo = this.currentSearchState.endDate.toISOString()
    
    parameters.set("filters", JSON.stringify(obj));
    const response = await fetch('weatherforecast?'+parameters);
    const data = await response.json();
    this.setState({ forecasts: data, loading: false });
  }

  exampleSearch = () => {
    if( this.state.startDate && this.state.endDate){
      console.log("Start Date:", this.state.startDate.toISOString());
      console.log("End Date:", this.state.endDate.toISOString());
    }
    console.log("Checked id:", this.state.selectedIds);
    console.log("Types :", this.state.selectedTypes);
  }


  handleDateChange = ([startDate,endDate]) => {
    this.setState({ startDate,endDate });
  }

  handleCheckboxChange = (id) => {
    this.setState(prevState => {
      const isSelected = prevState.selectedIds.includes(id);
      const updatedIds = isSelected
        ? prevState.selectedIds.filter(selectedId => selectedId !== id)
        : [...prevState.selectedIds, id];

      return { selectedIds: updatedIds };
    });
  }

  handleTypeCheckboxChange = (type) => {
    this.setState(prevState => {
      const isSelected = prevState.selectedTypes.includes(type);
      const updateTypes = isSelected
        ? prevState.selectedTypes.filter(selectedType => selectedType !== type)
        : [...prevState.selectedTypes, type];

      return { selectedTypes: updateTypes };
    });
  }
}
